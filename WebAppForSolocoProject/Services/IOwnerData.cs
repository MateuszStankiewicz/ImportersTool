using System.Collections.Generic;
using WebAppForSolocoProject.Models;

namespace WebAppForSolocoProject.Services
{
    public interface IOwnerData
    {
        IEnumerable<Owner> Owners { get; }
    }
}
