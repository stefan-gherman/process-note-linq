using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Codecool.ApplicationProcess.Entities;

namespace Codecool.ApplicationProcess.Data
{
    /// <summary>
    /// XML storage for app.
    /// </summary>
    public class XMLRepository : IApplicationRepository
    {
        private IList<Mentor> _mentors;
        private IList<Applicant> _applicants;
        private IList<Application> _applications;
        private IList<School> _schools;

        /// <summary>
        /// Initializes a new instance of the <see cref="XMLRepository"/> class.
        /// Constructor for XMLRepository
        /// </summary>
        public XMLRepository()
        {
            Seed();
        }

        /// <inheritdoc/>
        public int AmountOfApplicationAfter(DateTime date)
        {
            var filteredResult = _applicants.Where((app) => app.StartDate > date);
            return filteredResult.Count();
        }

        /// <inheritdoc/>
        public IEnumerable<Mentor> GetAllMentorFrom(City city)
        {
            var filteredResult = _mentors.Where((m) => m.City == city);
            return filteredResult;
        }

        /// <inheritdoc/>
        public IEnumerable<Mentor> GetAllMentorWhomFavoriteLanguage(string language)
        {
            var filteredResult = _mentors.Where((m) => m.ProgrammingLanguage.ToLower().Equals(language.ToLower()));
            return filteredResult;
        }

        /// <inheritdoc/>
        public IEnumerable<Applicant> GetApplicantsOf(string contactMentorName)
        {
            if (contactMentorName.Length < 3)
            {
                throw new ArgumentException("Parameter length is unsuitable, 3 or more chars.");
            }

            var filteredResult = _applications.Where((appl) => appl.Mentor.Nickname.Equals(contactMentorName));
            IList<Applicant> resultSet = new List<Applicant>();
            foreach (var result in filteredResult)
            {
                resultSet.Add(result.Applicant);
            }

            return resultSet;
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetAppliedStudentEmailList()
        {
            var resultSet = _applicants.Where(appl => appl.Status == ApplicationStatus.Applied)
                .Select(appl => appl.Email);
            return resultSet;
        }

        /// <summary>
        /// Function that returns an enum parameter from city baed on string.
        /// </summary>
        /// <param name="cityName">A string containing a potential city name.</param>
        /// <returns>A city enum member.</returns>
        public City GetCity(string cityName)
        {
            City city = City.Bucharest;
            switch (cityName)
            {
                case "Bucharest":
                    city = City.Bucharest;
                    break;
                case "Budapest":
                    city = City.Budapest;
                    break;
                case "Miskolc":
                    city = City.Miskolc;
                    break;
                case "Krakow":
                    city = City.Krakow;
                    break;
                case "Warsaw":
                    city = City.Warsaw;
                    break;
                default:
                    city = City.Bucharest;
                    break;
            }

            return city;
        }

        /// <summary>
        /// Gets status type.
        /// </summary>
        /// <param name="statusName"> A string with status.</param>
        /// <returns> a status Enum type member.</returns>
        public ApplicationStatus GetStatus(string statusName)
        {
            ApplicationStatus status = ApplicationStatus.Approved;
            switch (statusName)
            {
                case "Applied":
                    status = ApplicationStatus.Applied;
                    break;
                case "Approved":
                    status = ApplicationStatus.Approved;
                    break;
                case "Rejected":
                    status = ApplicationStatus.Rejected;
                    break;
                case "Cancelled":
                    status = ApplicationStatus.Cancelled;
                    break;

                default:
                    status = ApplicationStatus.Cancelled;
                    break;
            }

            return status;
        }

        /// <inheritdoc/>
        public void Seed()
        {
            _mentors = new List<Mentor>();
            _applicants = new List<Applicant>();
            _applications = new List<Application>();
            _schools = new List<School>();
            XmlDocument xml = new XmlDocument();
            xml.Load(@"Resources\Backup.xml");
            #region Seed Schools
            XmlNodeList xnlist = xml.SelectNodes("/Data/Schools/School");
            foreach (XmlNode xn in xnlist)
            {
                _schools.Add(new School(xn["Name"].InnerText, GetCity(xn["City"].InnerText), xn["Country"].InnerText));
            }
            #endregion
            #region Seed Mentors
            xnlist = xml.SelectNodes("/Data/Mentors/Mentor");
            foreach (XmlNode xn in xnlist)
            {
                Mentor mentor = new Mentor(xn["FirstName"].InnerText, xn["LastName"].InnerText);
                mentor.City = GetCity(xn["City"].InnerText);
                mentor.PhoneNumber = xn["PhoneNumber"].InnerText;
                mentor.Nickname = xn["Nickname"].InnerText;
                mentor.ProgrammingLanguage = xn["ProgrammingLanguage"].InnerText;

                _mentors.Add(mentor);
            }
            #endregion
            #region Seed Applicants
            xnlist = xml.SelectNodes("/Data/Applicants/Applicant");
            foreach (XmlNode xn in xnlist)
            {
                Applicant applicant = new Applicant(xn["FirstName"].InnerText, xn["LastName"].InnerText);
                applicant.ApplicationCode = int.Parse(xn["ApplicationCode"].InnerText);
                applicant.Email = xn["Email"].InnerText;
                applicant.PhoneNumber = xn["PhoneNumber"].InnerText;
                if (xn["StartDate"].InnerText != string.Empty)
                {
                    applicant.StartDate = DateTime.Parse(xn["StartDate"].InnerText);
                }

                applicant.Status = GetStatus(xn["Status"].InnerText);

                _applicants.Add(applicant);
            }
            #endregion
            #region Seed Appplication
            var xnlistMentor = xml.SelectNodes("/Data/Applications/Application/Mentor");
            var xnlistApplicant = xml.SelectNodes("/Data/Applications/Application/Applicant");
            var xnlistDate = xml.SelectNodes("/Data/Applications/Application");
            foreach (XmlNode xn in xnlistDate)
            {
                Application application = new Application();

                var innerMentor = xn.SelectSingleNode("Mentor");
                var innerApplicant = xn.SelectSingleNode("Applicant");
                var mentor = _mentors.Single(m => m.PhoneNumber.Equals(innerMentor["PhoneNumber"].InnerText));
                var applicant = _applicants.Single(app => app.PhoneNumber.Equals(innerApplicant["PhoneNumber"].InnerText));

                application.ApplicationDate = DateTime.Parse(xn["ApplicationDate"].InnerText);
                application.Mentor = mentor;
                application.Applicant = applicant;
                _applications.Add(application);
            }
            #endregion
        }
    }
}
