namespace agility.models
{
    public class InstanceRole
    {
		private int _roleID;

		public int RoleID
		{
			get { return _roleID; }
			set { _roleID = value; }
		}

		private string? _role;
		public bool IsGlobalRole { get; set; }
		public int Sort { get; set; }

		public string? Role
		{
			get { return _role; }
			set { _role = value; }
		}

		public string? Name
		{
			get { return _role; }
			set { _role = value; }
		}
	}
}
