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
3. You will need valid Agility CMS credentials to authenticate and obtain an access token.

### Authentication

Before using the SDK, you must authenticate against the Agility Management API to obtain a valid access token. This token is required for all subsequent API requests.

The authentication process uses OAuth 2.0 and requires multiple steps:

#### Step 1: Authorization Request

First, initiate the authorization flow by redirecting the user to the authorization endpoint. You can implement this in your .NET application:

```csharp
using System;
using System.Web;

public class AuthService 
{
    private const string AuthUrl = "https://mgmt.aglty.io/oauth/authorize";
    private const string TokenUrl = "https://mgmt.aglty.io/oauth/token";
    
    public string GetAuthorizationUrl(string redirectUri, string state)
    {
        var queryParams = HttpUtility.ParseQueryString(string.Empty);
        queryParams["response_type"] = "code";
        queryParams["redirect_uri"] = redirectUri;
        queryParams["state"] = state;
        queryParams["scope"] = "openid profile email offline_access";
        
        return $"{AuthUrl}?{queryParams}";
    }
}

// Usage: Redirect user to authorization URL
var authService = new AuthService();
var redirectUri = "YOUR_REDIRECT_URI"; // e.g., "https://yourapp.com/callback"
var state = "YOUR_STATE"; // Generate a unique state value for security
var authUrl = authService.GetAuthorizationUrl(redirectUri, state);

// Redirect user to authUrl (implementation depends on your application type)
```

#### Step 2: Exchange Authorization Code for Access Token

After successful authentication, you'll receive an authorization code at your redirect URI. Use this code to obtain an access token:

```csharp
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class TokenResponse
{
    public string access_token { get; set; }
    public string refresh_token { get; set; }
    public int expires_in { get; set; }
    public string token_type { get; set; }
}

public async Task<TokenResponse> ExchangeCodeForTokenAsync(string authorizationCode)
{
    using var httpClient = new HttpClient();
    
    var requestBody = new List<KeyValuePair<string, string>>
    {
        new("code", authorizationCode),
        new("grant_type", "authorization_code")
    };
    
    var request = new HttpRequestMessage(HttpMethod.Post, TokenUrl)
    {
        Content = new FormUrlEncodedContent(requestBody)
    };
    
    var response = await httpClient.SendAsync(request);
    var responseContent = await response.Content.ReadAsStringAsync();
    
    if (!response.IsSuccessStatusCode)
    {
        throw new HttpRequestException($"Token request failed: {responseContent}");
    }
    
    return JsonConvert.DeserializeObject<TokenResponse>(responseContent);
}

// Usage: Exchange authorization code for tokens
var tokenResponse = await ExchangeCodeForTokenAsync("YOUR_AUTHORIZATION_CODE");
var accessToken = tokenResponse.access_token;
```

#### Step 3: Refresh Token (Optional)

If you included `offline_access` in the scope, you can use the refresh token to obtain new access tokens:

```csharp
public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
{
    using var httpClient = new HttpClient();
    
    var requestBody = new List<KeyValuePair<string, string>>
    {
        new("refresh_token", refreshToken),
        new("grant_type", "refresh_token")
    };
    
    var request = new HttpRequestMessage(HttpMethod.Post, TokenUrl)
    {
        Content = new FormUrlEncodedContent(requestBody)
    };
    
    var response = await httpClient.SendAsync(request);
    var responseContent = await response.Content.ReadAsStringAsync();
    
    if (!response.IsSuccessStatusCode)
    {
        throw new HttpRequestException($"Token refresh failed: {responseContent}");
    }
    
    return JsonConvert.DeserializeObject<TokenResponse>(responseContent);
}
```

#### Step 4: Initialize the SDK

Use the obtained access token to initialize the SDK:

```csharp
using management.api.sdk;

// Initialize the Options Class with your obtained token
var options = new agility.models.Options
{
    token = accessToken, // Use the access_token from Step 2
    locale = "en-us",    // Your website locale
    guid = "your-website-guid" // Your website GUID
};

// Initialize the Client instance Class
var clientInstance = new ClientInstance(options);
```






### Security Considerations

- **State Parameter**: Always use a unique, unpredictable state parameter to prevent CSRF attacks
- **HTTPS**: Ensure all redirect URIs use HTTPS in production
- **Token Storage**: Store access and refresh tokens securely (encrypted storage, secure key management)
- **Token Expiration**: Implement proper token refresh logic before tokens expire

### Setup Options Class

Create an object of the Options class to provide values of:
- **token**: Bearer token obtained through the OAuth flow above
- **locale**: The locale under which your application is hosted (Example: "en-us")
- **guid**: The GUID under which your application is hosted

### Method Classes

Create an object of Method class(es), which can be used to create and perform operations. Following is the description of Classes and their respective methods:

### Making a Request

```csharp
using management.api.sdk;

// Initialize the Options Class with your authenticated token
var options = new agility.models.Options
{
    token = accessToken, // Token obtained from authentication flow above
    locale = "en-us",    // Your website locale
    guid = "your-website-guid" // Your website GUID
};

// Initialize the Client instance Class
var clientInstance = new ClientInstance(options);

// Make the request: get a content item with the ID '22'
var contentItem = await clientInstance.contentMethods.GetContentItem(22, options.guid, options.locale);
```
## Class AssetMethods
This class is used to perform operations related to Assets. The following are the methods: - 

### Upload
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `files` | `Dictionary<string,string>` | The key will be the file name and value will be the folder path of the files. The file should present at the local folder provided in the dictionary.  |
| `guid` | `string` | Current website guid.|
| `agilityFolderPath` | `string` | Path of the folder in Agility where the file(s) needs to be uploaded.|
| `groupingID` | `int` | Path of the folder in Agility where the file(s) needs to be uploaded.|

Returns: A collection of ```Media``` class Object.

### CreateFolder
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `originKey` | `Dictionary<string,string>` | The origin key of the requested folder.  |
| `guid` | `string` | Current website guid.|

Returns: A collection of ```Media``` class Object.

### DeleteFile
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `mediaID` | `int` | The mediaID of the asset which needs to be deleted.|
| `guid` | `string` | Current website guid.|
Returns
A ```string``` response if a file has been deleted.

### MoveFile
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `mediaID` | `int` | The mediaID of the file that needs to be moved.|
| `newFolder` | `string` | The new location (in Agility) where the file needs to be moved.|
| `guid` | `string` | Current website guid.|

Returns: An object of ```Media``` class with the new location of the file.

### GetMediaList
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `pageSize` | `int` | The page size on which the assets needs to selected.|
| `recordOffset` | `int` | The record offset value to skip search results.|
| `guid` | `string` | Current website guid.|

Returns: An object of ```AssetMediaList``` class.

### GetGalleryById
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `guid` | `string` | Current website guid.|
| `id` | `int` | The ID of the requested gallery.|

Returns: An object of ```AssetMediaGrouping``` class.

### GetGalleryByName
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `guid` | `string` | Current website guid.|
| `galleryName` | `string` | The name of the requested gallery.|

Returns: An object of ```AssetMediaGrouping``` class.

### GetDefaultContainer
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `guid` | `string` | Current website guid.|

Returns: An object of ```AssetContainer``` class.

### GetGalleries
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `guid` | `string` | Current website guid.|
| `search` | `string` | String to search a specific gallery item.|
| `pageSize` | `int` | The pageSize on which the galleries needs to be selected.|
| `rowIndex` | `int` | The rowIndex value for the resultant record set.|

Returns: An object of ```AssetGalleries``` class.

### SaveGallery
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `guid` | `string` | Current website guid.|
| `gallery` | `AssetMediaGrouping` | Object of AssetMediaGrouping class.|

Returns: An object of ```AssetMediaGrouping``` class.

### DeleteGallery
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `guid` | `string` | Current website guid.|
| `id` | `int` | The id of the gallery to be deleted.|

A ```string``` response if the gallery has been deleted.

### GetAssetByID
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `mediaID` | `int` | The mediaID of the requested asset.|
| `guid` | `string` | Current website guid.|

Returns: An object of ```Media``` class with the information of the asset.

### GetAssetByURL
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `url` | `string` | The url of the requested asset.|
| `guid` | `string` | Current website guid.|

Returns: An object of ```Media``` class with the information of the asset.

## Class BatchMethods
This class is used to perform operations related to Batches. The following are the methods: - 

### GetBatch
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `id` | `int` | The batchID of the requested batch.|
| `guid` | `string` | Current website guid.|

Returns: A object of ```Batch``` class.

## Class ContainerMethods
This class is used to perform operations related to Containers. The following are the methods: - 

### GetContainerById
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `id` | `int` | The container id of the requested container.|
| `guid` | `string` | Current website guid.|

Returns: A object of ```Container``` class.

### GetContainerByReferenceName
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `referenceName` | `string` | The container reference name of the requested container.|
| `guid` | `string` | Current website guid.|

Returns: A object of ```Container``` class.

### GetContainerSecurity
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `id` | `int` | The container id of the requested container.|
| `guid` | `string` | Current website guid.|

Returns: A object of ```Container``` class.

### GetContainerList
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `guid` | `string` | Current website guid.|
Returns: A collection object of ```Container``` class.

### GetNotificationList
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `id` | `int` | The container id of the requested container.|
| `guid` | `string` | Current website guid.|

Returns: A collection object of ```Notification``` class.

### SaveContainer
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `container` | `Container` | A Container type object to create or update a container.|
| `guid` | `string` | Current website guid.|

Returns: An object of ```Container``` class.

### DeleteContainer
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `id` | `int` | The container id of the requested container.|
| `guid` | `string` | Current website guid.|

Returns: A ```string``` response if a container has been deleted.

## Class ContentMethods
This class is used to perform operations related to Content. The following are the methods: - 

### GetContentItem
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `contentID` | `int` | The contentid of the requested content.|
| `guid` | `string` | Current website guid.|
| `locale` | `string` | Current website locale.|

Returns: An object of ```ContentItem``` class.

### PublishContent
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `contentID` | `int` | The contentid of the requested content.|
| `guid` | `string` | Current website guid.|
| `locale` | `string` | Current website locale.|
| `comments` | `string` | Additional comments for a batch request.|

Returns: The ```contentID``` of the requested content.

### UnPublishContent
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `contentID` | `int` | The contentid of the requested content.|
| `guid` | `string` | Current website guid.|
| `locale` | `string` | Current website locale.|
| `comments` | `string` | Additional comments for a batch request.|

Returns: The ```contentID``` of the requested content.

### ContentRequestApproval
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `contentID` | `int` | The contentid of the requested content.|
| `guid` | `string` | Current website guid.|
| `locale` | `string` | Current website locale.|
| `comments` | `string` | Additional comments for a batch request.|

Returns: The ```contentID``` of the requested content.

### ApproveContent
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `contentID` | `int` | The contentid of the requested content.|
| `guid` | `string` | Current website guid.|
| `locale` | `string` | Current website locale.|
| `comments` | `string` | Additional comments for a batch request.|

Returns: The ```contentID``` of the requested content.

### DeclineContent
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `contentID` | `int` | The contentid of the requested content.|
| `guid` | `string` | Current website guid.|
| `locale` | `string` | Current website locale.|
| `comments` | `string` | Additional comments for a batch request.|

Returns: The ```contentID``` of the requested content.

### SaveContentItem
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `contentItem` | `ContentItem` | A contentItem object to create or update a content.|
| `guid` | `string` | Current website guid.|
| `locale` | `string` | Current website locale.|

Returns: The ```contentID``` of the requested content.

### SaveContentItems
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `contentItems` | `List<ContentItem>` | A collection of contentItems object to create or update multiple contents.|
| `guid` | `string` | Current website guid.|
| `locale` | `string` | Current website locale.|

Returns: A ```list of object``` which consists of the processed contentID's for the batch request.

### DeleteContent
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `contentID` | `int` | The contentid of the requested content.|
| `guid` | `string` | Current website guid.|
| `locale` | `string` | Current website locale.|
| `comments` | `string` | Additional comments for a batch request.|

Returns: The ```contentID``` of the requested content.

### GetContentItems
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `referenceName` | `string` | The reference name of the container for the requested content.|
| `guid` | `string` | Current website guid.|
| `locale` | `string` | Current website locale.|
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
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `guid` | `string` | Current website guid.|
Returns: A collection of ```WebsiteUser``` class of the requested content.

### SaveUser
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `emailAddress` | `string` | The email address of the requested user.|
| `roles` | `List<InstanceRole>` | Collection object of InstanceRole class for the requested user.|
| `guid` | `string` | Current website guid.|
| `firstName` | `string` | The first name of the requested user.|
| `lastName` | `string` | The last name of the requested user.|

Returns: An object of the ```InstanceUser``` class.

### DeleteUser
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `userID` | `int` | The userID of the requested user.|
| `guid` | `string` | Current website guid.|

Returns: A ```string``` response if a user has been deleted.

## Class ModelMethods
This class is used to perform operations related to Models. The following are the methods: - 

### GetContentModel
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `id` | `int` | The id of the requested model.|
| `guid` | `string` | Current website guid.|

Returns: An object of ```Model``` class.

### GetModelByReferenceName
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `referenceName` | `string` | The referenceName of the requested model.|
| `guid` | `string` | The guid of the requested model.|

Returns: An object of ```Model``` class.

### GetContentModules
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `includeDefaults` | `bool` | Boolean value to include defaults.|
| `guid` | `string` | Current website guid.|
| `includeModules` | `bool` | Boolean value to include modules.|

Returns: A collection object of ```Model``` class.

### GetPageModules
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `guid` | `string` | Current website guid.|
| `includeDefault` | `bool` | Boolean value to include defaults.|

Returns: A collection object of ```Model``` class.

### SaveModel
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `model` | `Model` | The object of Model to for the requested model.|
| `guid` | `string` | Current website guid.|

Returns: An object of ```Model``` class.

### DeleteModel
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `id` | `int` | The id for the requested model.|
| `guid` | `string` | Current website guid.|

Returns: A ```string``` response if a model is deleted.

## Class PageMethods
This class is used to perform operations related to Pages. The following are the methods: - 

### GetSiteMap
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `guid` | `string` | Current website guid.|
| `locale` | `string` | Current website locale.|
Returns: A collection object of ```Sitemap``` class.

### GetPage
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `pageID` | `int` | The id of the requested page.|
| `guid` | `string` | Current website guid.|
| `locale` | `string` | Current website locale.|

Returns: An object of ```PageItem``` class.

### PublishPage
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `pageID` | `int` | The pageID of the requested page.|
| `guid` | `string` | Current website guid.|
| `locale` | `string` | Current website locale.|
| `comments` | `string` | Additional comments for a batch request.|

Returns: The ```pageID``` of the requested page.

### UnPublishPage
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `pageID` | `int` | The pageID of the requested page.|
| `guid` | `string` | Current website guid.|
| `locale` | `string` | Current website locale.|
| `comments` | `string` | Additional comments for a batch request.|

Returns: The ```pageID``` of the requested page.

### DeletePage
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `pageID` | `int` | The pageID of the requested page.|
| `guid` | `string` | Current website guid.|
| `locale` | `string` | Current website locale.|
| `comments` | `string` | Additional comments for a batch request.|

Returns: The ```pageID``` of the requested page.

### ApprovePage
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `pageID` | `int` | The pageID of the requested page.|
| `guid` | `string` | Current website guid.|
| `locale` | `string` | Current website locale.|
| `comments` | `string` | Additional comments for a batch request.|

Returns: The ```pageID``` of the requested page.

### DeclinePage
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `pageID` | `int` | The pageID of the requested page.|
| `guid` | `string` | Current website guid.|
| `locale` | `string` | Current website locale.|
| `comments` | `string` | Additional comments for a batch request.|

Returns: The ```pageID``` of the requested page.

### PageRequestApproval
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `pageID` | `int` | The pageID of the requested page.|
| `guid` | `string` | Current website guid.|
| `locale` | `string` | Current website locale.|
| `comments` | `string` | Additional comments for a batch request.|

Returns: The ```pageID``` of the requested page.

### SavePage
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `pageItem` | `PageItem` | The object of PageItem class for the requested Page.|
| `guid` | `string` | Current website guid.|
| `locale` | `string` | Current website locale.|
| `parentPageID` | `int` | The id of the parent page.|
| `placeBeforePageItemID` | `int` | The id of the page before the page.|

Returns: The ```pageID``` of the requested page.


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
