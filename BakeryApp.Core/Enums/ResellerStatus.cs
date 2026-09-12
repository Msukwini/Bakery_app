namespace BakeryApp.Core.Enums;

public enum ResellerStatus
{
    TRIAL,       // 0 — new reseller on trial
    ACTIVE,      // 1 — full reseller
    SUSPENDED,   // 2 — temporarily blocked
    INACTIVE,    // 3 — stopped selling but not terminated
    TERMINATED   // 4 — permanent end
}
