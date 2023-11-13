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
    public async Task<ActionResult<PhotoDto>> Get(Guid photoId)
    {
        var photo = await _context.Photos.FindAsync(photoId);

        if (photo == null) {
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
