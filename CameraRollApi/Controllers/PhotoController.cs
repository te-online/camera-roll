using Microsoft.AspNetCore.Mvc;
using Domain.Entities;

namespace CameraRollApi.Controllers;

[ApiController]
[Route("[controller]")]
public class PhotoController : ControllerBase
{
    private readonly ILogger<PhotoController> _logger;

    public PhotoController(ILogger<PhotoController> logger)
    {
        _logger = logger;
    }

    [HttpGet(
        "{photoId}",
        Name = "GetPhoto")
    ]
    public Photo Get(Guid photoId)
    {
        var photo = new Photo();
        return photo;
    }

    [HttpGet(
        "Feed",
        Name = "GetPhotos"
    )]
    public IEnumerable<Photo> GetPhotos()
    {
        Photo[] photos = { new() };
        return photos;
    }
}
