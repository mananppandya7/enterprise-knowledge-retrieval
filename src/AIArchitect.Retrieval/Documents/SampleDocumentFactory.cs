using AIArchitect.Retrieval.Models;

namespace AIArchitect.Retrieval.Documents;

/// <summary>
/// Creates the small, synthetic corpus defined by the assessment brief.
/// The documents are structured like their source formats so later chunkers can make
/// document-specific decisions without requiring PDF or spreadsheet parsers in the prototype.
/// </summary>
public static class SampleDocumentFactory
{
    public static IReadOnlyList<SourceDocument> CreateAll() =>
    [
        CreateStarModernizationSow(),
        CreateHealthcareDistributorCaseStudy(),
        CreateManufacturingCaseStudy(),
        CreateEcommerceRegressionTestPlan(),
        CreateAiTransformationProposal()
    ];

    private static SourceDocument CreateStarModernizationSow() => SourceDocument.Create(
        id: "star-modernization-sow",
        fileName: "SOW_STAR_Modernization_Assessment.pdf",
        documentType: DocumentType.StatementOfWork,
        title: "STAR Platform Modernization - Statement of Work",
        content:
        """
        # STAR Platform Modernization - Statement of Work

        ## 1. Background
        The client operates a mature ERP platform used for customer management, purchasing,
        warehouse operations, equipment tracking, invoicing, and laboratory workflows. The
        existing desktop application is difficult to maintain and does not meet current security
        or scalability expectations.

        ## 2. Objectives
        Modernize the application interface and service layer while preserving familiar business
        workflows and existing database formats. Improve performance, auditability, role-based
        access, and supportability without forcing a disruptive replacement of the core database.

        ## 3. Scope
        Deliver a browser-based user interface and .NET API for the customer, sales, purchasing,
        warehouse, equipment, laboratory, and administration modules. Implement field-level
        permissions, audit history, reporting, tax calculation, shipping-carrier integration, and
        label printing.

        ## 4. Modernization Approach
        The delivery team will migrate forms in five controlled sprints. Each sprint includes
        static analysis, vulnerability testing, automated regression coverage, client review, and
        a two-week user-acceptance window. Existing workflows remain recognizable to reduce
        training effort and adoption risk.

        ## 5. Data Migration
        The current relational schema remains the system of record. Schema changes require joint
        approval. Migration scripts will normalize invalid reference data, preserve record keys,
        reconcile row counts, and produce an exception report before go-live.

        ## 6. Deliverables
        Deliverables include source code, deployment scripts, permission matrices, audit tables,
        integration adapters, test evidence, operating documentation, and four weeks of hypercare.

        ## 7. Assumptions
        The client will provide cloud environments, repository access, subject-matter experts,
        third-party credentials, and timely decisions. Database performance issues in unchanged
        legacy routines remain the client's responsibility.

        ## 8. Out of Scope
        Replacing the ERP database, redesigning core accounting policy, predictive analytics,
        autonomous decision-making, and changes to third-party products are excluded.
        """,
        metadata: Metadata(
            ("client", "Client Co."),
            ("engagement", "STAR modernization"),
            ("industry", "enterprise software"),
            ("assessment-content", "synthetic")));

    private static SourceDocument CreateHealthcareDistributorCaseStudy() => SourceDocument.Create(
        id: "healthcare-distributor-case-study",
        fileName: "CaseStudy_HealthcareDistributor.pdf",
        documentType: DocumentType.CaseStudy,
        title: "Healthcare Distributor - Digital Transformation Case Study",
        content:
        """
        # Healthcare Distributor - Digital Transformation Case Study

        ## Client Profile
        A United States healthcare distributor supplies mobility aids, orthopedic products,
        rehabilitation equipment, and patient-lifting solutions. It serves clinicians, veterans,
        and consumers through several distribution centers and fulfills approximately 7,000
        orders each month.

        ## Business Challenge
        Five full-time representatives manually re-entered orders from multiple channels into the
        ERP. Inventory records lagged behind warehouse activity, product search was unreliable,
        and inconsistent order prioritization caused fulfillment delays. Teams spent time chasing
        exceptions instead of supporting customers, while medical products could be allocated to
        the wrong warehouse or replenished too late.

        ## AI and Automation Solution
        The delivery team implemented an intelligent order-operations platform for the healthcare
        client. Robotic process automation captured and validated incoming orders, while anomaly
        detection flagged incomplete, duplicated, or clinically sensitive requests for review.
        Demand-forecasting models used order history and regional patterns to recommend inventory
        levels for high-use medical and therapeutic products. An inventory recommendation service
        proposed warehouse transfers and replenishment before shortages affected fulfillment.

        Orders were prioritized using delivery commitments, product availability, and customer
        needs. Automated shipping orchestration selected the appropriate distribution center and
        carrier. A customer-service assistant surfaced order status and approved product details to
        representatives, with human review retained for exceptions and regulated decisions.

        ## Technical Approach
        The solution combined a scalable eCommerce storefront, ERP integration APIs, event-driven
        order workflows, RPA workers, and governed forecasting services. Monitoring measured model
        drift, automation confidence, exception rates, and inventory accuracy. Role-based access
        and audit logs protected customer and operational data.

        ## Outcomes
        Manual processing time fell by 67 percent and automation achieved 91 percent accuracy.
        Work equivalent to 3.5 full-time representatives shifted to customer and exception support,
        producing estimated annual savings of $184,000. More accurate stock recommendations and
        warehouse routing shortened fulfillment time, while the modernized portal improved product
        discovery and customer experience.
        """,
        metadata: Metadata(
            ("client", "anonymized healthcare distributor"),
            ("industry", "healthcare distribution"),
            ("solution-type", "AI, RPA, eCommerce, ERP"),
            ("delivery-status", "delivered"),
            ("assessment-content", "synthetic")));

    private static SourceDocument CreateManufacturingCaseStudy() => SourceDocument.Create(
        id: "manufacturing-case-study",
        fileName: "CaseStudy_Manufacturing.pdf",
        documentType: DocumentType.CaseStudy,
        title: "Manufacturing and Automotive Brand - Dealer Service Transformation",
        content:
        """
        # Manufacturing and Automotive Brand - Dealer Service Transformation

        ## Client Profile
        A global manufacturing and automotive brand supports a large network of dealers,
        suppliers, warranty teams, and service partners.

        ## Business Challenge
        Dealer requests moved through disconnected email inboxes and regional systems. There was
        no central history for customer inquiries, follow-ups, service notes, or warranty claims.
        Manual handoffs delayed responses and made performance difficult to measure.

        ## Solution
        The delivery team built a secure dealer portal integrated with the client's existing CRM.
        A centralized workflow engine routed customer, dealer, supplier, and warranty requests to
        accountable teams. Real-time ticket tracking gave dealers a consistent view of progress,
        while a dedicated warranty module standardized evidence collection and approvals.

        ## Integration
        APIs synchronized dealer identities, CRM cases, warranty records, attachments, and status
        changes. Role-based access limited each dealer to its own accounts and cases. Operational
        dashboards highlighted queues, aging tickets, and service-level breaches.

        ## Outcomes
        Average dealer response time improved from 48 hours to 4 hours. Customer satisfaction rose
        by 78 percent and operational overhead fell by 38 percent. The unified portal established a
        scalable foundation for consistent global service delivery.
        """,
        metadata: Metadata(
            ("client", "anonymized manufacturing brand"),
            ("industry", "manufacturing and automotive"),
            ("solution-type", "CRM and dealer portal"),
            ("delivery-status", "delivered"),
            ("assessment-content", "synthetic")));

    private static SourceDocument CreateEcommerceRegressionTestPlan() => SourceDocument.Create(
        id: "ecommerce-regression-test-plan",
        fileName: "Regression_Test_Plan_eCommerce.xlsx",
        documentType: DocumentType.TestPlan,
        title: "Retail eCommerce Regression Test Plan",
        content:
        """
        # Workbook Summary
        Platform: Retail eCommerce
        Version: 1.0
        Coverage: 240 test cases across Checkout, Inventory Management, and Returns & Refunds
        Execution states: Not Run, In Progress, Pass, Fail, Blocked

        ## Sheet: Checkout
        Test ID | Module | Scenario | Expected Result | Priority
        CHK-001 | Cart Management | Add an in-stock item to cart | Item and subtotal appear correctly | P1
        CHK-017 | Authentication | Session expires during checkout | User re-authenticates and cart is preserved | P2
        CHK-032 | Payment | Submit a declined credit card | Clear error appears and no order is created | P1
        CHK-059 | Order Placement | Place an order for two units | Order is created and available stock decreases by two | P1
        CHK-074 | Security | Enter script markup in an address field | Input is rejected or safely encoded | P1

        ## Sheet: Inventory
        Test ID | Module | Scenario | Expected Result | Priority
        INV-001 | Stock Levels | Receive a purchase order | Available inventory increases by received quantity | P1
        INV-014 | Reservations | Reserve the final unit during checkout | Unit cannot be allocated to another order | P1
        INV-031 | Multi-Warehouse | Transfer stock between warehouses | Both locations and audit history update | P1
        INV-052 | Replenishment | Stock falls below reorder threshold | Replenishment notification is created | P2
        INV-077 | Concurrency | Two users purchase the final unit | Only one order succeeds without overselling | P1

        ## Sheet: Returns
        Test ID | Module | Scenario | Expected Result | Priority
        RET-002 | Eligibility | Request return inside the allowed window | Return authorization is created | P1
        RET-019 | Refunds | Approve a card refund | Correct amount returns to original payment method | P1
        RET-036 | Inventory | Receive a resellable returned item | Available stock increases after inspection | P1
        RET-061 | Exceptions | Return an item from a cancelled order | Duplicate refund is prevented | P1
        RET-080 | Reporting | Export monthly return metrics | Export totals match transaction records | P3
        """,
        metadata: Metadata(
            ("platform", "retail eCommerce"),
            ("test-case-count", "240"),
            ("modules", "checkout, inventory, returns"),
            ("assessment-content", "synthetic")));

    private static SourceDocument CreateAiTransformationProposal() => SourceDocument.Create(
        id: "ai-transformation-proposal",
        fileName: "AI_Transformation_Proposal.pdf",
        documentType: DocumentType.SalesProposal,
        title: "AI Transformation Proposal - Operational Automation",
        content:
        """
        # AI Transformation Proposal - Operational Automation

        ## Executive Summary
        This proposal identifies where practical AI automation could reduce manual re-entry,
        information gaps, contract errors, and delayed cash collection across the client's order
        and production lifecycle. The recommendations augment existing job-management and
        accounting platforms rather than replacing them.

        ## Sales and Quoting
        A pre-visit intake assistant could gather customer requirements before a site visit. With
        consent, call transcription and field extraction could draft quote details and identify
        missing contract information while the salesperson is still with the customer.

        ## Ordering and Procurement
        Document-reading automation could capture delivery notes and vendor bills. Inventory-first
        purchasing rules could check available material before drafting a purchase order, then
        recommend vendors using current pricing, lead time, and discount thresholds.

        ## Production and Installation
        A scheduling assistant could verify deposit and material readiness before proposing an
        installation sequence. Event-driven updates could reserve consumed material and inform
        customers when an order enters fabrication or installation.

        ## Financial Close
        Completion events could trigger final-payment requests, accounting tasks, and exception
        alerts. A management dashboard could consolidate outstanding balances, inventory value,
        upcoming installations, and slow-moving stock.

        ## Expected Benefits
        The proposed program targets faster cash conversion, fewer incomplete contracts, more
        accurate inventory, and less administrative effort. Benefits are estimates to validate
        during a phased proof of concept; this proposal does not represent a completed delivery.
        """,
        metadata: Metadata(
            ("client", "anonymized commercial client"),
            ("industry", "construction supply and installation"),
            ("solution-type", "proposed AI automation"),
            ("delivery-status", "proposal"),
            ("assessment-content", "synthetic")));

    private static IEnumerable<KeyValuePair<string, string>> Metadata(
        params (string Key, string Value)[] entries) =>
        entries.Select(entry => KeyValuePair.Create(entry.Key, entry.Value));
}
