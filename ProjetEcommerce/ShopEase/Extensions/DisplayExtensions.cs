using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using ShopEase.Models;

namespace ShopEase.Extensions;

public static class DisplayExtensions
{
    private static readonly CultureInfo Fr = CultureInfo.GetCultureInfo("fr-FR");

    /// <summary>Formate un montant en dinars : 1 234,50 DT.</summary>
    public static string ToDt(this decimal amount) => amount.ToString("N2", Fr) + " DT";

    public static string ToFrDate(this DateTime utc) => utc.ToLocalTime().ToString("dd/MM/yyyy", Fr);
    public static string ToFrDateTime(this DateTime utc) => utc.ToLocalTime().ToString("dd/MM/yyyy à HH:mm", Fr);

    /// <summary>Renvoie le libellé [Display(Name)] d'une valeur d'énumération.</summary>
    public static string DisplayName(this Enum value) =>
        value.GetType().GetMember(value.ToString()).FirstOrDefault()
            ?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? value.ToString();

    public static string BadgeClass(this OrderStatus s) => s switch
    {
        OrderStatus.Pending => "bg-warning text-dark",
        OrderStatus.Paid => "bg-info text-dark",
        OrderStatus.Shipped => "bg-primary",
        OrderStatus.Delivered => "bg-success",
        _ => "bg-secondary"
    };
}
