// Stripe checkout test — run with: dotnet run
// Usage:  dotnet run [apiKey] [priceId]
// If no args, falls back to the hardcoded values below.

using Stripe;
using Stripe.Checkout;

var apiKey  = args.ElementAtOrDefault(0)
    ?? "sk_test_51SJ3Xb4BF2uOrrJbVIm8OTvuK3vvM95s3ykjHsXvvBQI6EqLml5eExfpsTtSAlDyht80h4cnrGnQBi8rHd1bZxzZ00Rkt5z0jS";

var priceId = args.ElementAtOrDefault(1)
    ?? "price_1TESUk4BF2uOrrJboMK98ytd";

Console.WriteLine($"API key : {apiKey[..10]}...");
Console.WriteLine($"Price ID: {priceId}");
Console.WriteLine();

StripeConfiguration.ApiKey = apiKey;

try
{
    // 1. Create a throwaway customer so we can test setup_future_usage
    Console.WriteLine("Creating test Stripe customer...");
    var custService = new CustomerService();
    var customer = await custService.CreateAsync(new CustomerCreateOptions
    {
        Name     = "Stripe Test Customer",
        Metadata = new Dictionary<string, string> { ["test"] = "true" }
    });
    Console.WriteLine($"Customer: {customer.Id}");

    // 2. Create a Checkout Session (same options the app uses)
    Console.WriteLine("Creating checkout session...");
    var sessionService = new SessionService();
    var session = await sessionService.CreateAsync(new SessionCreateOptions
    {
        Customer  = customer.Id,
        Mode      = "payment",
        LineItems = [new SessionLineItemOptions { Price = priceId, Quantity = 1 }],
        Metadata  = new Dictionary<string, string>
        {
            ["test"]          = "true",
            ["purchase_type"] = "stripe_test",
        },
        PaymentIntentData = new SessionPaymentIntentDataOptions
        {
            SetupFutureUsage = "off_session",
            Description      = "Stripe test checkout"
        },
        AllowPromotionCodes = true,
        SuccessUrl = "http://localhost:5200/app/test-success?session_id={CHECKOUT_SESSION_ID}",
        CancelUrl  = "http://localhost:5200/app/test-cancel",
    });

    Console.WriteLine();
    Console.WriteLine("SUCCESS — open this URL in your browser:");
    Console.WriteLine();
    Console.WriteLine(session.Url);
    Console.WriteLine();
}
catch (StripeException ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine($"STRIPE ERROR [{ex.HttpStatusCode}]: {ex.StripeError?.Message ?? ex.Message}");
    Console.Error.WriteLine($"Error type   : {ex.StripeError?.Type}");
    Console.Error.WriteLine($"Error code   : {ex.StripeError?.Code}");
    Console.ResetColor();
    Environment.Exit(1);
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine($"ERROR: {ex.Message}");
    Console.ResetColor();
    Environment.Exit(1);
}
