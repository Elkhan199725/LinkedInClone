using Application.Common.Interfaces;
using Domain.Entities;

namespace Infrastructure.Persistence.Repositories;

public sealed class PostMediaRepository : BaseRepository<PostMedia>, IPostMediaRepository
{
    public PostMediaRepository(ApplicationDbContext context) : base(context)
    {
    }
}
