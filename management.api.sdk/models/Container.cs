using agility.enums;

namespace agility.models
{
    public class Container
    {
        public List<ContentViewColumn> Columns { get; set; }
        public int? ContentViewID { get; set; }
        public int? ContentDefinitionID { get; set; }
        public string? ContentDefinitionName { get; set; }
        
        public string? ReferenceName { get; set; }
        public string? ContentViewName { get; set; }
        public int ContentDefinitionType { get; set; }
        
        public bool? RequiresApproval { get; set; }
        
        public string? LastModifiedDate { get; set; }
        
        public DateTime? LastModifiedOn { get; set; }
        
        public string? LastModifiedBy { get; set; }
        
        public bool? IsShared { get; set; }
        
        public bool? IsDynamicPageList { get; set; }
        
        public bool? DisablePublishFromList { get; set; }
        
        public int? ContentViewCategoryID { get; set; }
        
        public string? ContentViewCategoryReferenceName { get; set; }
        
        public string? ContentViewCategoryName { get; set; }
        
        public string? title { get; set; }
        
        public string? defaultPage { get; set; }
        
        public bool? isPublished { get; set; }
        
        public string? schemaTitle { get; set; }
        
        public bool? allowClientSideSave { get; set; }
        
        public string? defaultSortColumn { get; set; }
        
        public string? defaultSortDirection { get; set; }
        
        public int? usageCount { get; set; }
        
        public bool? isDeleted { get; set; }
        
        public bool? enableRSSOutput { get; set; }
        
        public bool? enableAPIOutput { get; set; }
        
        public string? commentsRecordTypeName { get; set; }
        
        public int? numRowsInListing { get; set; }
        
        public int? ContentDefinitionTypeID { get; set; } = (int)ContentModelType.List;
        
        public DateTime? fullSyncModDate { get; set; }
        
        public bool? confirmSharingOnPublish { get; set; }
        public string? ContentTemplateName { get; set; }
        
        public bool? currentUserCanDelete { get; set; }
        
        public bool? currentUserCanEdit { get; set; }
        
        public bool? currentUserCanDesign { get; set; }
        
        public bool? currentUserCanManage { get; set; }
        
        public bool? currentUserCanContribute { get; set; }
        
        public bool? currentUserCanPublish { get; set; }
        
        public string? defaultListingPage
        {
            get
            {
                if (string.IsNullOrEmpty(defaultPage)) return string.Empty;

                string s = "";
                if (defaultPage.IndexOf("|") != -1)
                {
                    s = defaultPage.Substring(0, defaultPage.IndexOf("|"));

                }
                else
                {
                    s = defaultPage;
                }

                if (s.IndexOf("^") != -1)
                {
                    return s.Substring(0, s.IndexOf("^"));
                }
                else
                {
                    return s;
                }
            }
            set
            {

                if (!string.IsNullOrEmpty(defaultPage) && defaultPage.IndexOf("|") != -1)
                {
                    defaultPage = value + defaultPage.Substring(defaultPage.IndexOf("|"));
                }
                else
                {
                    defaultPage = value;
                }
            }
        }
        
        public string? defaultDetailsPage
        {
            get
            {
                if (string.IsNullOrEmpty(defaultPage)) return string.Empty;

                string s = "";
                if (defaultPage.IndexOf("|") != -1)
                {
                    s = defaultPage.Substring(defaultPage.IndexOf("|") + 1);

                }
                else
                {
                    s = defaultPage;
                }

                if (s.IndexOf("^") != -1)
                {
                    return s.Substring(0, s.IndexOf("^"));
                }
                else
                {
                    return s;
                }
            }

            set
            {

                if (!string.IsNullOrEmpty(defaultPage) && defaultPage.IndexOf("|") != -1)
                {
                    defaultPage = defaultPage.Substring(0, defaultPage.IndexOf("|") + 1) + value;
                }
                else if (!string.IsNullOrEmpty(defaultPage))
                {
                    defaultPage = defaultPage + "|" + value;
                }
                else
                {
                    defaultPage = value;
                }
            }
        }
        
        public string DefaultDetailsPageQueryString
        {
            get
            {
                if (string.IsNullOrEmpty(defaultPage)) return string.Empty;

                if (defaultPage.IndexOf("^") != -1)
                {
                    string s = defaultPage.Substring(defaultPage.IndexOf("^") + 1);
                    return s;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                if (!string.IsNullOrEmpty(defaultPage) && defaultPage.IndexOf("^") != -1)
                {
                    defaultPage = defaultPage.Substring(0, defaultPage.IndexOf("^") + 1) + value;
                }
                else if (!string.IsNullOrEmpty(defaultPage))
                {
                    defaultPage = defaultPage + "^" + value;
                }
                else
                {
                    defaultPage = "|^" + value;
                }
            }
        }
        
        public bool? isListItem { get; set; }
    }

    public class ContentViewColumn
    {
        public int? SortOrder { get; set; }
        public string? FieldName { get; set; }
        public string? Label { get; set; }
        public bool? IsDefaultSort { get; set; }
        public string? SortDirection { get; set; }
        public string? TypeName { get; set; }
    }
}
