using Abp.Application.Navigation;
using Abp.Authorization;
using Abp.Localization;
using pbt.Authorization;

namespace pbt.Web.Startup
{
    /// <summary>
    /// This class defines menus for the application.
    /// </summary>
    public class pbtNavigationProvider : NavigationProvider
    {
        
        public override void SetNavigation(INavigationProviderContext context)
        {
            
            context.Manager.MainMenu
                .AddItem(
                    new MenuItemDefinition(
                        PageNames.Home,
                        L("HomePage"),
                        url: "",
                        icon: "fas fa-home",
                        requiresAuthentication: true
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        PageNames.Users,
                        L("Users"),
                        url: "Users",
                        icon: "fas fa-users",
                        permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Users)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        PageNames.Roles,
                        L("Roles"),
                        url: "Roles",
                        icon: "fas fa-theater-masks",
                        permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Roles)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        PageNames.Dictionarys,
                        L("Dictionaries"),
                        url: "Dictionarys",
                        icon: "fas fa-theater-masks",
                        permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Dictionaries)
                    )
                )
                .AddItem(
                    new MenuItemDefinition(
                        PageNames.ShipingPartners,
                        L("ShippingPartner"),
                        url: "ShippingPartners",
                        icon: "fas fa-theater-masks",
                        permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_ShippingPartners)
                    )
                )
                .AddItem(
                    new MenuItemDefinition(
                        PageNames.Warehouses,
                        L("Warehouse"),
                        url: "Warehouses",
                        icon: "fas fa-theater-masks",
                        permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Warehouses)
                    )
                )
                ;
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, pbtConsts.LocalizationSourceName);
        }
    }
}