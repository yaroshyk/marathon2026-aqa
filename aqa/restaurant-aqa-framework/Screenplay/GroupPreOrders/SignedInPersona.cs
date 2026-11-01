namespace Restaurant_Aqa_Framework.Screenplay.GroupPreOrders;

/// <summary>A persona's id token, remembered under the persona's name so each one signs in once per scenario.</summary>
public sealed record SignedInPersona(Persona Persona, string IdToken);
