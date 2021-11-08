using System.Linq;
using DqtApi.Models;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace DqtApi.DAL
{
    public class DataverseAdaptor : IDataverseAdaptor
    {
        private readonly DqtServiceContext _context;

        public DataverseAdaptor(IOrganizationServiceAsync organizationServiceAsync)
        {
            _context = new DqtServiceContext(organizationServiceAsync);
        }

        public Contact GetTeacher(GetTeacherRequest request)
        {
            if (string.IsNullOrEmpty(request.NationalInsuranceNumber))
            {
                return GetTeacherByTRN(request);
            }
            else
            {
                return GetTeacherByTRNOrNationalInsuranceNumber(request);
            }
        }

        private Contact GetTeacherByTRN(GetTeacherRequest request)
        { 
            return (from contact in _context.ContactSet
                    where contact.dfeta_TRN == request.TRN
                    && contact.BirthDate == request.BirthDate
                    select contact).SingleOrDefault();
        }

        private Contact GetTeacherByTRNOrNationalInsuranceNumber(GetTeacherRequest request)
        {
            return (from contact in _context.ContactSet
                    where (contact.dfeta_NINumber == request.NationalInsuranceNumber || contact.dfeta_TRN == request.TRN)
                    && contact.BirthDate == request.BirthDate
                    select contact).SingleOrDefault();
        }
    }
}
