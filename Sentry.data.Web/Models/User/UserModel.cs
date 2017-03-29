using Sentry.data.Core;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sentry.data.Web
{
    public class UserModel
    {
        /// <summary>
        /// Parameterless constructor is needed for view binding
        /// </summary>
        public UserModel()
        {

        }

        public UserModel(IApplicationUser user)
        {
            this.Id = user.DomainUser.Id;
            this.Ranking = user.DomainUser.Ranking;
            this.AssociateId = user.DomainUser.AssociateId;
            this.FullName = user.DisplayName;

        }

        public int Id { get; set; }

        [Required()]
        public int Ranking { get; set; }

        [DisplayName("Associate Id")]
        public string AssociateId { get; set; }

        [DisplayName("Full Name")]
        public string FullName { get; set; }

    }
}
