namespace CoreApiRest.BusinessRules;

public static class CotizacionErrors
{
    public const string PesoFueraDeRango = "BR-02";

    public const string TipoClienteNoSoportado = "BR-03";

    public const string TipoEnvioNoSoportado = "BR-04";

    public const string SolicitudInvalida = "VALIDATION";

    public static Error SolicitudObligatoria() =>
        new(SolicitudInvalida, "La solicitud es obligatoria.", ErrorType.Validation);

    public static Error PesoObligatorio() =>
        new(SolicitudInvalida, "El peso es obligatorio.", ErrorType.Validation);

    public static Error TipoClienteObligatorio() =>
        new(SolicitudInvalida, "El tipo de cliente es obligatorio.", ErrorType.Validation);

    public static Error TipoEnvioObligatorio() =>
        new(SolicitudInvalida, "El tipo de envío es obligatorio.", ErrorType.Validation);

    public static Error ClienteNoSoportado() =>
        new(TipoClienteNoSoportado, "El tipo de cliente no está soportado. Valores permitidos: Regular, Corporativo, VIP.", ErrorType.BusinessRule);

    public static Error EnvioNoSoportado() =>
        new(TipoEnvioNoSoportado, "El tipo de envío no está soportado. Valores permitidos: Estandar, Express.", ErrorType.BusinessRule);

    public static Error PesoInvalido(string detalle) =>
        new(PesoFueraDeRango, detalle, ErrorType.BusinessRule);
}
