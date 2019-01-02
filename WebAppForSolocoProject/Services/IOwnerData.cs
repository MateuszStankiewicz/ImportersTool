using System.Collections.Generic;
using WebAppForSolocoProject.Models;

namespace WebAppForSolocoProject.Services
{
    public interface IOwnerData
    {
        Owner GetOwner(string name);
        IEnumerable<Owner> GetAllOwners();

        IEnumerable<string> GetAllPaths(Owner owner);
    }
}
