using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;
using System;

namespace Somo.API.Entities
{
    
    [CollectionName("Users")]
    public class ApplicationUser : MongoIdentityUser<Guid>
    {
        
    }
}