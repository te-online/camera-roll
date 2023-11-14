using Microsoft.AspNetCore.Mvc;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using TinifyAPI;
using Amazon.S3;
using Amazon.S3.Model;

namespace CameraRollApi.Controllers;

[ApiController]
[Route("[controller]")]
public class PhotoController : ControllerBase
{
    private readonly ILogger<PhotoController> _logger;
    private readonly PhotoContext _context;
    private readonly IConfiguration _config;

    public PhotoController(ILogger<PhotoController> logger, PhotoContext context, IConfiguration config)
    {
        _logger = logger;
        _context = context;
        _config = config;
    }

    [HttpGet(
        "{photoId}",
        Name = "GetPhoto")
    ]
    public async Task<ActionResult<PhotoDto>> GetPhoto(Guid photoId)
    {
        var photo = await _context.Photos
            .Where(x => x.Id == photoId)
            .Where(x => x.IsPublic == true)
            .FirstAsync();

        if (photo == null) 
        {
            return NotFound();
        }

        return ItemToDTO(photo);
    }

    [HttpGet(
        "Feed",
        Name = "GetPhotos"
    )]
    public async Task<ActionResult<IEnumerable<PhotoDto>>> GetPhotos()
    {
        return await _context.Photos
            .Where(x => x.IsPublic == true)
            .Select(x => ItemToDTO(x))
            .ToListAsync();
    }

    [HttpPut("{photoId}")]
    public async Task<IActionResult> PutPhoto(Guid photoId, PhotoUpdateDto photoDto)
    {
        var photo = await _context.Photos.FindAsync(photoId);
        if (photo == null)
        {
            return NotFound();
        }

        photo.Title = photoDto.Title;
        photo.Description = photoDto.Description;
        photo.IsPublic = photoDto.IsPublic == true;
        photo.UpdatedAt = DateTime.Now;

        try
        {
            var result = await _context.SaveChangesAsync();
            return CreatedAtAction(
                nameof(GetPhoto),
                new { photoId = photo.Id },
                ItemToDTO(photo));
        }
        catch (DbUpdateConcurrencyException) when (!PhotoExists(photoId))
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult<PhotoDto>> PostPhoto(PhotoCreateDto photoDto)
    {
        byte[] imageBytes = Convert.FromBase64String(photoDto.Image);
        Guid photoId = Guid.NewGuid();
        
        var source = Tinify.FromBuffer(imageBytes);
        var adjustedSource = source
            .Convert(new 
            {
                type = new []{"image/webp"}
            })
            .Resize(new 
            {
                method = "cover",
                width = 1920,
                height = 1080
            });
        var result = await adjustedSource.GetResult();

        var config = new AmazonS3Config
        {
            ServiceURL = "https://s3.eu-central-003.backblazeb2.com"
        };

        AmazonS3Client s3Client = new(
            _config["AWS:S3_ACCESS_KEY"],
            _config["AWS:S3_SECRET_KEY"],
            config
        );

        PutObjectRequest request = new()
        {
            BucketName = "photos-thomasebert-net",
            Key = photoId + "." + result.Extension,
            ContentType = result.ContentType,
            InputStream = new MemoryStream(result.ToBuffer())
        };

        try 
        {
            await s3Client.PutObjectAsync(request);
        } 
        catch (AmazonS3Exception e)
        {
            _logger.LogError("Error encountered on server. Message:'{Message}' when writing an object", e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("Unknown error encountered on server. Message:'{Message}' when writing an object", e.Message);
        }

        var photo = new Photo 
        {
            Id = photoId,
            S3Url = "https://" + request.BucketName + ".s3.eu-central-003.backblazeb2.com/" + photoId + "." + result.Extension
        };

        _context.Photos.Add(photo);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetPhoto),
            new { photoId = photo.Id },
            ItemToDTO(photo));
    }

    [HttpDelete("{photoId}")]
    public async Task<IActionResult> DeletePhoto(Guid photoId)
    {
        var photo = await _context.Photos.FindAsync(photoId);
        if (photo == null)
        {
            return NotFound();
        }

        _context.Photos.Remove(photo);
        await _context.SaveChangesAsync();

        return Ok(
            new Photo
            {
                Id = photoId
            }
        );
    }
    
    private bool PhotoExists(Guid photoId)
    {
        return _context.Photos.Any(e => e.Id == photoId);
    }

    private static PhotoDto ItemToDTO(Photo photo) =>
       new()
       {
           Id = photo.Id,
           Description = photo.Description,
           S3Url = photo.S3Url,
           Title = photo.Title,
           UpdatedAt = photo.UpdatedAt,
           CreatedAt = photo.CreatedAt
       };
}
