using Microsoft.AspNetCore.Mvc;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CameraRollApi.Controllers;

[ApiController]
[Route("[controller]")]
public class PhotoController : ControllerBase
{
    private readonly ILogger<PhotoController> _logger;
    private readonly PhotoContext _context;

    public PhotoController(ILogger<PhotoController> logger, PhotoContext context)
    {
        _logger = logger;
        _context = context;
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

        var photo = new Photo 
        {
            S3Url = "https://example.org/here-goes-the-photo"
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
