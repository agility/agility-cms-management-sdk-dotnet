namespace agility.models
{
    public class InstanceUser
    {
		public int UserID { get; set; }
		public string? EmailAddress { get; set; }
		public string? FirstName { get; set; }

		public string? LastName { get; set; }

		public bool IsDeleted { get; set; }
		public bool IsTeamUser { get; set; }
		public string? DefaultUILanguage { get; set; }

		protected string userName = "";
		public string UserName
		{
			get { return userName; }
			set { userName = value; }
		}

		protected List<InstanceRole> _InstanceRoles = new List<InstanceRole>();
		public List<InstanceRole> InstanceRoles
		{
			get
			{
				if (_InstanceRoles == null) _InstanceRoles = new List<InstanceRole>();
				return _InstanceRoles;
			}
			set
			{
				_InstanceRoles = value;
			}
		}

		protected List<InstancePermission>? _InstancePermissions = null;
		public List<InstancePermission> InstancePermissions
		{
			get
			{
				if (_InstancePermissions == null) _InstancePermissions = new List<InstancePermission>();
				return _InstancePermissions;
			}
			set
			{
				_InstancePermissions = value;
			}
		}

	}
}
