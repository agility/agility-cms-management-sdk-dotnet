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
