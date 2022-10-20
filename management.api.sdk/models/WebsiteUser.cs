namespace agility.models
{
    public class WebsiteUser
    {
        public int? UserID { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? EmailAddress { get; set; }
        public bool IsDeleted { get; set; }
		public string? FullName { get; set; }
		public bool IsTeamUser { get; set; }
        public bool IsSuspended { get; set; }

		public int? TeamID { get; set; }

	
		protected List<InstanceRole> _userRoles = new List<InstanceRole>();
		public List<InstanceRole> UserRoles
		{
			get
			{
				if (_userRoles == null) _userRoles = new List<InstanceRole>();
				return _userRoles;
			}
			set
			{
				_userRoles = value;
			}
		}

		protected List<InstancePermission> _userPermissions = null;
		public List<InstancePermission> UserPermissions
		{
			get
			{
				if (_userPermissions == null) _userPermissions = new List<InstancePermission>();
				return _userPermissions;
			}
			set
			{
				_userPermissions = value;
			}
		}

		public DateTime? LoginDate { get; set; }

		public bool IsOrgAdmin { get; set; }
	}

	public class InstancePermission
	{
		string? _permissionType;
		int _permissionID;
		string? _name;

		public string? PermissionType
		{
			get { return _permissionType; }
			set { _permissionType = value; }
		}

		public int PermissionID
		{
			get { return _permissionID; }
			set { _permissionID = value; }
		}

		public string? Name
		{
			get { return _name; }
			set { _name = value; }
		}

	}
}
