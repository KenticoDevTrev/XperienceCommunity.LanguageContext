# XperienceCommunity.LanguageContext

## Description

This package is to provide an optimized (cached) current culture/language retrieval package to use both on sites, along with various other packages (since many needed this type of logic).

It provides two IServiceCollection extensions, `AddLanguageContext` (to be used by the website, which configures both the Retriever and the Setter), and a `AddLanguageContextForDependentLibrary` which can safetly be used on any extension and will gaurentee that the `ILanguageContextRetriever` will be available and working.

## Description

This is a community created package to allow Member Roles and permissions in Xperience by Kentico (until this logic is baked into the product).

Roles can be created, assigned to Members, and Content Items can be secured with Authentication and Member Role Permissions applied to themselves or inherited from a parent Content Folders and Web Page Items.

## Library Version Matrix (XperienceCommunity.LanguageContext)

This project is using [Xperience Version v30.0.0](https://docs.kentico.com/changelog#refresh-december-12-2024).

| Xperience Version  | Library Version |
| ------------------ | --------------- |
| >= 30.0.*          | 1.0.0           |


## Library Version Matrix (XperienceCommunity.LanguageContext.KX13)

This project is using [Kentico.Xperience.AspNetCore.WebApp v13.0.130](https://www.nuget.org/packages/Kentico.Xperience.AspNetCore.WebApp).

| Kentico Xperience 13 Version  | Library Version |
| ----------------------------- | --------------- |
| >= 13.0.130                   | 1.0.0           |

## Package Installation (Xperience by Kentico)

Add the package to your application using the .NET CLI
```powershell
dotnet add package XperienceCommunity.LanguageContext
```

Additionally, you can elect to install only the required packages on specific projects if you have separation of concerns:

**XperienceCommunity.LanguageContext.Core** : No Xperience Dependencies

## Package Installation (Kentico Xperience 13)

Add the package to your application using the .NET CLI
```powershell
dotnet add package XperienceCommunity.LanguageContext.KX13
```

Additionally, you can elect to install only the required packages on specific projects if you have separation of concerns:

**XperienceCommunity.LanguageContext.Core** : No Xperience Dependencies

## Quick Start
In your startup, on your IServiceCollection, use the extension method `AddLanguageContext` or `AddLanguageContextForDependentLibrary`.

**AddLanguageContext**: This is to be used by the website, and two optional Option Actions to configure.  The defaults will behave how Xperience by Kentico/KX13 behave (assuming you do not set the ___Name properties)

**AddLanguageContextForDependentLibrary**: This only adds the `ILanguageContextRetriever` with default options ***If it hasn't already been set by the website application***.

Additionally, for the website only, you can leverage the `IApplicationBuilder.UseLanguageContext` middleware (after Kentico's Routing) to set the language context to the HttpContext.Items so further retrieval is instant.

## Customizations
You can customize the behavior through the `LanguageContextOptions` in the `AddLanguageContext` method.  This gives you the option to set the `SourcePriority` (allowing you to leverage things like a language query string, cookie, or header IF you set the adjacent QueryStringName, CookieName, HeaderName Properties).

The `LanguageNameRouteValuesKey` is the Route Key that the language is stored in, and is mainly only used in Xperience by Kentico, and only becuase currently I cannot access the WebPageRoutingOption (which you can set a different value).  It defaults to `WebPageRoutingOptions.LANGUAGE_ROUTE_VALUE_KEY` so most likely you won't need to set this **unless** you change the key value in your Kentico configuration startup.

## Notes on KX13 and XbyK Differences

While most behavior is the same between the two, Xperience by Kentico is a little more complex.

The `LanguageSource.LanguageRouteOrThreadCulture` checks the Language Route (Xperience by Kentico), while in Kentico Xperience 13 it uses the ThreadCulture because documentation states this is how you should proceed.

Xperience by Kentico also has a Default Content Language (ContentLanguageIsDefault), where as Kentico Xperience 13 uses the Settings Key only (with ability to change it per site).  The Site has an optional Default Visitor Culture which it does leverage.

## Contributing

Please feel free to create a pull request if you find any bugs.  If you are on the Xperience Community Slack, that's the best place to hit me up so I get eyes on it.

## License

Distributed under the MIT License. See [`LICENSE.md`](./LICENSE.md) for more
information.
