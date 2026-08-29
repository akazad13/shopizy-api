using Shopizy.Domain.Permissions.ValueObjects;
using Shopizy.Domain.Users;
using Shopizy.Domain.Users.Entities;
using Shopizy.Domain.Users.Enums;
using Shouldly;
using Xunit;

namespace Shopizy.Domain.UnitTests.Users;

public class NotificationPreferencesTests
{
    [Fact]
    public void NotificationPreference_CreateDefault_ShouldHaveAllChannelsEnabled()
    {
        var prefs = NotificationPreference.CreateDefault();

        prefs.EmailEnabled.ShouldBeTrue();
        prefs.PushEnabled.ShouldBeTrue();
        prefs.OrderUpdates.ShouldBeTrue();
        prefs.Promotions.ShouldBeTrue();
        prefs.PriceAlerts.ShouldBeTrue();
        prefs.RestockAlerts.ShouldBeTrue();
    }

    [Fact]
    public void NotificationPreference_Create_WithCustomValues_ShouldSetPropertiesCorrectly()
    {
        var prefs = NotificationPreference.Create(
            emailEnabled: false,
            pushEnabled: false,
            orderUpdates: true,
            promotions: false,
            priceAlerts: true,
            restockAlerts: false
        );

        prefs.EmailEnabled.ShouldBeFalse();
        prefs.PushEnabled.ShouldBeFalse();
        prefs.OrderUpdates.ShouldBeTrue();
        prefs.Promotions.ShouldBeFalse();
        prefs.PriceAlerts.ShouldBeTrue();
        prefs.RestockAlerts.ShouldBeFalse();
    }

    [Fact]
    public void NotificationPreference_Update_ShouldModifyValues()
    {
        var prefs = NotificationPreference.CreateDefault();

        prefs.Update(
            emailEnabled: false,
            pushEnabled: false,
            orderUpdates: false,
            promotions: false,
            priceAlerts: false,
            restockAlerts: false
        );

        prefs.EmailEnabled.ShouldBeFalse();
        prefs.PushEnabled.ShouldBeFalse();
        prefs.OrderUpdates.ShouldBeFalse();
        prefs.Promotions.ShouldBeFalse();
        prefs.PriceAlerts.ShouldBeFalse();
        prefs.RestockAlerts.ShouldBeFalse();
    }

    [Fact]
    public void User_UpdateNotificationPreferences_ShouldUpdateUserPreferences()
    {
        var user = User.Create(
            "First",
            "Last",
            "user@test.com",
            "hash",
            UserRole.Customer,
            new List<PermissionId>()
        );
        user.NotificationPreferences.ShouldNotBeNull();
        user.NotificationPreferences.EmailEnabled.ShouldBeTrue();

        user.UpdateNotificationPreferences(
            emailEnabled: false,
            pushEnabled: true,
            orderUpdates: true,
            promotions: false,
            priceAlerts: true,
            restockAlerts: false
        );

        user.NotificationPreferences.EmailEnabled.ShouldBeFalse();
        user.NotificationPreferences.PushEnabled.ShouldBeTrue();
        user.NotificationPreferences.OrderUpdates.ShouldBeTrue();
        user.NotificationPreferences.Promotions.ShouldBeFalse();
        user.NotificationPreferences.PriceAlerts.ShouldBeTrue();
        user.NotificationPreferences.RestockAlerts.ShouldBeFalse();
    }
}
