namespace Payment.Domain.Enums;
public enum VnpResponseCode
{
    Success = 00,
    SuspiciousTransaction = 07,
    NotRegisteredInternetBanking = 09,
    IncorrectAuthentication = 10,
    PaymentTimeout = 11,
    AccountLocked = 12,
    IncorrectOTP = 13,
    TransactionCancelled = 24,
    InsufficientFunds = 51,
    ExceededTransactionLimit = 65,
    BankMaintenance = 75,
    IncorrectPaymentPassword = 79,
    OtherErrors = 99
}
