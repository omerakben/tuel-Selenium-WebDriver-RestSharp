# Client-Specific Element Analysis & Cleanup Plan

## Executive Summary
This document identifies all bank client-specific elements that need to be generalized for open-source release.

## 🚨 Critical Client-Specific Elements

### 1. **File Names Requiring Rename**

#### Current → Generic
- `Beneficiaries.cs` → `Members.cs` or `Customers.cs`
- `BeneficiariesPOM.cs` → `MembersPOM.cs` or `CustomersPOM.cs`
- `CompletedPudLines.cs` → `TransactionDetails.cs` or `ActivityDetails.cs`
- `PudLinesPOM.cs` → `TransactionDetailsPOM.cs`
- `Completed.cs` → `Transactions.cs` or `Activity.cs`
- `CompletedPOM.cs` → `TransactionsPOM.cs`
- `Fees.cs` → `Pricing.cs` or `Fees.cs` (if generic enough)
- `FeesPOM.cs` → `PricingPOM.cs`

### 2. **Client-Specific Domain Terms**

#### Terms to Replace
- "Beneficiaries" → "Members" or "Users"
- "PUD Lines" → "Transaction Details" or "Line Items"
- "Completed Items" → "Transaction History" or "Activity Log"
- "TUEL" → "Products" (already in some places)
- "Business Application" → "Dashboard" or "Application"
- "Credit Portal" → "Member Portal" or "User Portal"

### 3. **Specific Column Headers & Data Fields**

#### In BeneficiariesPOM.cs:
```csharp
// Current (Client-Specific)
"View", "Beneficiary", "Address", "Address 2", "Address 3", "City", "State", "Zip Code"

// Generic
"View", "Member", "Address", "Address 2", "City", "State", "Zip Code"
```

#### In CompletedPOM.cs:
```csharp
// Current (Client-Specific)
"View", "DDA", "Member", "Issue Date", "Expiration Date", "Beneficiary", "Amount", "TUEL #", "TUEL Type"

// Generic
"View", "Account", "Member", "Issue Date", "Expiration Date", "Recipient", "Amount", "Product #", "Product Type"
```

### 4. **Navigation & Tab Names**

#### Current Terms:
- "Beneficiaries Tab" → "Members Tab"
- "Completed Tab" → "Transactions Tab"
- "PUD Lines Sub-Tab" → "Transaction Details Sub-Tab"
- "Special TUEL Templates" → "Special Product Templates"

### 5. **Business Logic Terms**

#### API Models:
- `ProductDetail` → Keep (generic)
- `ProductListItem` → Keep (generic)
- `StatusHistoryItem` → Keep (generic)
- `Customer` model → Keep (generic)

#### Page-Specific Terms:
- "Fee Activity" → "Activity" or "Fee Management"
- "Business Document Signatories" → "Document Signers" or "Signature Management"

## 📋 Files Requiring Significant Changes

### High Priority (Core Client Logic)
1. `Beneficiaries.cs` - Rename + content update
2. `BeneficiariesPOM.cs` - Rename + content update
3. `CompletedPudLines.cs` - Rename + content update
4. `PudLinesPOM.cs` - Rename + content update
5. `Completed.cs` - Content update (minor)
6. `CompletedPOM.cs` - Content update (minor)
7. `Fees.cs` - Content update
8. `FeesPOM.cs` - Content update
9. `TemplatesPOM.cs` - "TUEL" references
10. `MembersPOM.cs` - "Credit Portal" reference

### Medium Priority (Navigation & References)
11. `Base.cs` - Any references to renamed pages
12. `Dashboard.cs` - Navigation tab references
13. `Templates.cs` - "TUEL" references
14. `Completed.cs` - Tab navigation references

### Low Priority (Already Generic)
15. `Dashboard.cs` - Mostly generic
16. `HealthCheck.cs` - Generic
17. `Login.cs` - Generic
18. `Members.cs` - Already uses "Members"

## 🔄 Recommended Generic Mapping

### Page Mapping
| Original      | Generic             | Reason                             |
| ------------- | ------------------- | ---------------------------------- |
| Beneficiaries | Members             | Generic customer/member management |
| Completed     | Transactions        | Generic transaction history        |
| PUD Lines     | Transaction Details | Generic line item details          |
| Fees          | Pricing             | Generic pricing/fee management     |
| Templates     | Templates           | Keep as-is (generic)               |
| Dashboard     | Dashboard           | Keep as-is (generic)               |
| Members       | Members             | Keep as-is (generic)               |
| HealthCheck   | HealthCheck         | Keep as-is (generic)               |
| Login         | Login               | Keep as-is (generic)               |

### Entity Mapping
| Original    | Generic     | Example              |
| ----------- | ----------- | -------------------- |
| Beneficiary | Member/User | Customer management  |
| TUEL        | Product     | Product management   |
| DDA         | Account     | Account management   |
| Completion  | Transaction | Transaction tracking |

## 🎯 Cleanup Phases

### Phase 1: File Renaming
- Rename test class files
- Rename POM files
- Update namespaces if needed

### Phase 2: Content Replacement
- Replace client-specific strings
- Update method names and comments
- Update test descriptions

### Phase 3: Navigation Updates
- Update tab navigation references
- Update URL patterns
- Update header texts

### Phase 4: Documentation
- Update README files
- Update inline documentation
- Update examples

### Phase 5: Verification
- Ensure all tests reference generic names
- Verify no hardcoded client names
- Check configuration files

## ⚠️ Critical Considerations

### 1. **No Bank Client References**
- Remove "bank", "financial services", "letter of credit"
- Remove client-specific acronyms
- Remove proprietary business logic

### 2. **Generic Business Logic**
- Convert to customer management
- Generic transaction tracking
- Universal product management

### 3. **Domain-Neutral Examples**
- Products, Orders, Customers (already implemented)
- Generic workflows
- Reusable patterns

### 4. **Configuration Independence**
- No hardcoded client URLs
- Environment-agnostic setup
- Flexible configuration

## 📊 Current vs. Proposed Structure

### Test Classes
```
Current:
- Beneficiaries.cs ❌
- Completed.cs ❌
- CompletedPudLines.cs ❌
- Fees.cs ❌

Proposed:
- Members.cs ✅
- Transactions.cs ✅
- TransactionDetails.cs ✅
- Pricing.cs ✅
```

### POM Classes
```
Current:
- BeneficiariesPOM.cs ❌
- CompletedPOM.cs ❌
- PudLinesPOM.cs ❌
- FeesPOM.cs ❌

Proposed:
- MembersPOM.cs ✅
- TransactionsPOM.cs ✅
- TransactionDetailsPOM.cs ✅
- PricingPOM.cs ✅
```

## 🚀 Next Steps

1. ✅ **Analysis Complete** - Client-specific elements identified
2. ⏳ **Awaiting Approval** - User review of proposed changes
3. ⏳ **Execute Phase 1** - File renaming
4. ⏳ **Execute Phase 2** - Content replacement
5. ⏳ **Execute Phase 3** - Navigation updates
6. ⏳ **Execute Phase 4** - Documentation updates
7. ⏳ **Execute Phase 5** - Final verification

---

**Ready to proceed?** Please review this analysis and confirm the proposed generic mappings before I begin the cleanup.

