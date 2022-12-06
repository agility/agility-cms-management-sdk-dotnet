# Agility CMS & Management API .Net SDK

To start using the Agility CMS & .NET 5 Starter, [sign up](https://agilitycms.com/free) for a FREE account and create a new Instance using the Blog Template.

[Introduction to .NET and Agility CMS](https://help.agilitycms.com/hc/en-us/articles/4404089239693)

## About the Management API SDK

- Uses the latest version of .NET, with greatly improved performance across many components, Language improvements to C# and F#, and much more.
- Provides a facility to developers to use the new Agility Management API more effectively.
- Provides methods to perform operations on Assets, Batches, Containers, Content, Models, Pages, and Users.
- Supports the creation of Pages and Content in batches.
- Ability to generate Content in bulk for a Website.

## Getting Started

### Prerequisites
1. Clone the solution agility-cms-management-sdk-dotnet.
2. Add namespace management.api.sdk to make use of the Options class.
3. Create an object of the Options class to provide values of - 
	- token -> Bearer token to authenticate a Rest Request to perform an operation.
	- locale -> The locale under which your application is hosted. Example en-us.
    - guid -> The guid under which your application is hosted.
4. Create an object of Method class(es), which can be used to create and perform operations. Following is the description of Classes and their respective methods -

### Making a Request
```C#
using management.api.sdk;

//initialize the Options Class
agility.models.Options options = new  agility.models.Options();

options.token = "<<Provide Auth Token>>";
options.guid = "<<Provide the Guid of the Website>>";
options.locale = "<<Provide the locale of the Website>>"; //Example: en-us

//Initialize the Method Class
ContentMethods contentMethods = new ContentMethods(options);

//make the request: get a content item with the ID '22'
var contentItem = await contentMethods.GetContentItem(22);
```
## Class AssetMethods
This class is used to perform operations related to Assets. The following are the methods: - 

### Upload
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `files` | `Dictionary<string,string>` | The key will be the file name and value will be the folder path of the files. The file should present at the local folder provided in the dictionary.  |
| `agilityFolderPath` | `string` | Path of the folder in Agility where the file(s) needs to be uploaded.|
| `groupingID` | `int` | Path of the folder in Agility where the file(s) needs to be uploaded.|

Returns: A collection of ```Media``` class Object.

### DeleteFile
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `mediaID` | `int` | The mediaID of the asset which needs to be deleted.|
Returns
A ```string``` response if a file has been deleted.

### MoveFile
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `mediaID` | `int` | The mediaID of the file that needs to be moved.|
| `newFolder` | `string` | The new location (in Agility) where the file needs to be moved.|

Returns: An object of ```Media``` class with the new location of the file.

### GetMediaList
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `pageSize` | `int` | The page size on which the assets needs to selected.|
| `recordOffset` | `int` | The record offset value to skip search results.|

Returns: An object of ```AssetMediaList``` class.

### GetAssetByID
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `mediaID` | `int` | The mediaID of the requested asset.|

Returns: An object of ```Media``` class with the information of the asset.

### GetAssetByURL
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `url` | `string` | The url of the requested asset.|

Returns: An object of ```Media``` class with the information of the asset.

## Class BatchMethods
This class is used to perform operations related to Batches. The following are the methods: - 

### GetBatch
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `id` | `int` | The batchID of the requested batch.|

Returns: A object of ```Batch``` class.

## Class ContainerMethods
This class is used to perform operations related to Containers. The following are the methods: - 

### GetContainerById
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `id` | `int` | The container id of the requested container.|

Returns: A object of ```Container``` class.

### GetContainerByReferenceName
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `referenceName` | `string` | The container reference name of the requested container.|

Returns: A object of ```Container``` class.

### GetContainerSecurity
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `id` | `int` | The container id of the requested container.|

Returns: A object of ```Container``` class.

### GetContainerList
Returns: A collection object of ```Container``` class.

### GetNotificationList
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `id` | `int` | The container id of the requested container.|

Returns: A collection object of ```Notification``` class.

### SaveContainer
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `container` | `Container` | A Container type object to create or update a container.|

Returns: An object of ```Container``` class.

### DeleteContainer
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `id` | `int` | The container id of the requested container.|

Returns: A ```string``` response if a container has been deleted.

## Class ContentMethods
This class is used to perform operations related to Content. The following are the methods: - 

### GetContentItem
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `contentID` | `int` | The contentid of the requested content.|

Returns: An object of ```ContentItem``` class.

### PublishContent
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `contentID` | `int` | The contentid of the requested content.|
| `comments` | `string` | Additional comments for a batch request.|

Returns: A ```int contentID``` of the requested content.

### UnPublishContent
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `contentID` | `int` | The contentid of the requested content.|
| `comments` | `string` | Additional comments for a batch request.|

Returns: A ```int contentID``` of the requested content.

### ContentRequestApproval
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `contentID` | `int` | The contentid of the requested content.|
| `comments` | `string` | Additional comments for a batch request.|

Returns: A ```int contentID``` of the requested content.

### ApproveContent
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `contentID` | `int` | The contentid of the requested content.|
| `comments` | `string` | Additional comments for a batch request.|

Returns: A ```int contentID``` of the requested content.

### DeclineContent
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `contentID` | `int` | The contentid of the requested content.|
| `comments` | `string` | Additional comments for a batch request.|

Returns: A ```int contentID``` of the requested content.

### SaveContentItem
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `contentItem` | `ContentItem` | A contentItem object to create or update a content.|

Returns: A ```int contentID``` of the requested content.

### SaveContentItems
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `contentItems` | `List<ContentItem>` | A collection of contentItems object to create or update multiple contents.|

Returns: A ```list of object``` which consists of the processed contentID's for the batch request.

### DeleteContent
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `contentID` | `int` | The contentid of the requested content.|
| `comments` | `string` | Additional comments for a batch request.|

Returns: A ```int contentID``` of the requested content.

### GetContentItems
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `referenceName` | `string` | The reference name of the container for the requested content.|
| `filter` | `string` | The filter condition for the requested content.|
| `fields` | `string` | The fields mapped to the container.|
| `sortDirection` | `string` | The direction to sort the result.|
| `sortField` | `string` | The field on which the sort needs to be performed.|
| `take` | `int` | The page size for the result.|
| `skip` | `int` | The record offset for the result.|

Returns: An object of ```ContentList``` class of the requested content.

## Class InstanceUserMethods
This class is used to perform operations related to User. The following are the methods: - 

### GetUsers
Returns: A collection of ```WebsiteUser``` class of the requested content.

### SaveUser
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `emailAddress` | `string` | The email address of the requested user.|
| `roles` | `List<InstanceRole>` | Collection object of InstanceRole class for the requested user.|
| `firstName` | `string` | The first name of the requested user.|
| `lastName` | `string` | The last name of the requested user.|

Returns: An object of the ```InstanceUser``` class.

### DeleteUser
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `userID` | `int` | The userID of the requested user.|

Returns: A ```string``` response if a user has been deleted.

## Class ModelMethods
This class is used to perform operations related to Models. The following are the methods: - 

### GetContentModel
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `id` | `int` | The id of the requested model.|

Returns: An object of ```Model``` class.

### GetContentModules
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `includeDefaults` | `bool` | Boolean value to include defaults.|
| `includeModules` | `bool` | Boolean value to include modules.|

Returns: A collection object of ```Model``` class.

### GetPageModules
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `includeDefault` | `bool` | Boolean value to include defaults.|

Returns: A collection object of ```Model``` class.

### SaveModel
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `model` | `Model` | The object of Model to for the requested model.|

Returns: An object of ```Model``` class.

### DeleteModel
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `id` | `int` | The id for the requested model.|

Returns: A ```string``` response if a model is deleted.

## Class PageMethods
This class is used to perform operations related to Pages. The following are the methods: - 

### GetSiteMap
Returns: A collection object of ```Sitemap``` class.

### GetPage
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `pageID` | `int` | The id of the requested page.|

Returns: An object of ```PageItem``` class.

### PublishPage
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `pageID` | `int` | The pageID of the requested page.|
| `comments` | `string` | Additional comments for a batch request.|

Returns: A ```int pageID``` of the requested page.

### UnPublishPage
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `pageID` | `int` | The pageID of the requested page.|
| `comments` | `string` | Additional comments for a batch request.|

Returns: A ```int pageID``` of the requested page.

### DeletePage
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `pageID` | `int` | The pageID of the requested page.|
| `comments` | `string` | Additional comments for a batch request.|

Returns: A ```int pageID``` of the requested page.

### ApprovePage
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `pageID` | `int` | The pageID of the requested page.|
| `comments` | `string` | Additional comments for a batch request.|

Returns: A ```int pageID``` of the requested page.

### DeclinePage
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `pageID` | `int` | The pageID of the requested page.|
| `comments` | `string` | Additional comments for a batch request.|

Returns: A ```int pageID``` of the requested page.

### PageRequestApproval
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `pageID` | `int` | The pageID of the requested page.|
| `comments` | `string` | Additional comments for a batch request.|

Returns: A ```int pageID``` of the requested page.

### SavePage
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `pageItem` | `PageItem` | The object of PageItem class for the requested Page.|
| `parentPageID` | `int` | The id of the parent page.|
| `placeBeforePageItemID` | `int` | The id of the page before the page.|

Returns: A ```int pageID``` of the requested page.


## Running the SDK Locally

- `dotnet build` => Builds your .NET project.
- `dotnet run` => Builds & runs your .NET project.
- `dotnet clean` => Cleans the build outputs of your .NET project.

## How It Works

- [How Pages Work](https://help.agilitycms.com/hc/en-us/articles/4404222849677)
- [How Page Modules Work](https://help.agilitycms.com/hc/en-us/articles/4404222989453)
- [How Page Templates Work](https://help.agilitycms.com/hc/en-us/articles/4404229108877)

## Resources

### Agility CMS

- [Official site](https://agilitycms.com)
- [Documentation](https://help.agilitycms.com/hc/en-us)

### .NET 6

- [Official site](https://dotnet.microsoft.com/)
- [Documentation](https://docs.microsoft.com/en-us/dotnet/)

### Community

- [Official Slack](https://join.slack.com/t/agilitycommunity/shared_invite/enQtNzI2NDc3MzU4Njc2LWI2OTNjZTI3ZGY1NWRiNTYzNmEyNmI0MGZlZTRkYzI3NmRjNzkxYmI5YTZjNTg2ZTk4NGUzNjg5NzY3OWViZGI)
- [Blog](https://agilitycms.com/resources/posts)
- [GitHub](https://github.com/agility)
- [Forums](https://help.agilitycms.com/hc/en-us/community/topics)
- [Facebook](https://www.facebook.com/AgilityCMS/)
- [Twitter](https://twitter.com/AgilityCMS)

## Feedback and Questions

If you have feedback or questions about this starter, please use the [Github Issues](https://github.com/agility/agility-cms-management-sdk-dotnet/issues) on this repo, join our [Community Slack Channel](https://join.slack.com/t/agilitycommunity/shared_invite/enQtNzI2NDc3MzU4Njc2LWI2OTNjZTI3ZGY1NWRiNTYzNmEyNmI0MGZlZTRkYzI3NmRjNzkxYmI5YTZjNTg2ZTk4NGUzNjg5NzY3OWViZGI) or create a post on the [Agility Developer Community](https://help.agilitycms.com/hc/en-us/community/topics).
