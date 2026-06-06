namespace MediConnect.Domain.Enums;

public enum UserRole
{
    SuperAdmin = 0,
    ClinicAdmin = 1,
    Doctor = 2,
    Receptionist = 3,
    Patient = 4
}

public enum AppointmentStatus
{
    Requested = 0,
    Approved = 1,
    Rejected = 2,
    Rescheduled = 3,
    CheckedIn = 4,
    InProgress = 5,
    Completed = 6,
    Cancelled = 7,
    NoShow = 8
}

public enum AppointmentType
{
    GeneralConsultation = 0,
    RoutineCheckup = 1,
    DentalCleaning = 2,
    SurgeryConsultation = 3,
    FollowUpVisit = 4,
    EmergencyVisit = 5,
    RootCanal = 6,
    OrthodonticTreatment = 7,
    ImplantConsultation = 8,
    Other = 99
}

public enum DayOfWeekEnum
{
    Sunday = 0,
    Monday = 1,
    Tuesday = 2,
    Wednesday = 3,
    Thursday = 4,
    Friday = 5,
    Saturday = 6
}

public enum SubscriptionPlanTier
{
    Starter = 0,
    Professional = 1,
    Business = 2,
    Enterprise = 3
}

public enum SubscriptionStatus
{
    Trialing = 0,
    Active = 1,
    PastDue = 2,
    Cancelled = 3,
    Expired = 4
}

public enum PaymentMode
{
    Cash = 0,
    Upi = 1,
    Card = 2,
    NetBanking = 3
}

public enum InvoiceStatus
{
    Draft = 0,
    Issued = 1,
    PartiallyPaid = 2,
    Paid = 3,
    Cancelled = 4,
    Refunded = 5
}

public enum GenderType
{
    Unknown = 0,
    Male = 1,
    Female = 2,
    Other = 3
}

public enum NotificationChannel
{
    Email = 0,
    WhatsApp = 1,
    Sms = 2
}
