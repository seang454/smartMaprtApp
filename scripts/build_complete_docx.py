import docx
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT, WD_ALIGN_VERTICAL
from docx.oxml import OxmlElement, parse_xml
from docx.oxml.ns import qn, nsdecls

def create_document():
    doc = docx.Document()

    # --- Page Setup ---
    section = doc.sections[0]
    section.page_width = Inches(8.5)
    section.page_height = Inches(11.0)
    section.top_margin = Inches(0.8)
    section.bottom_margin = Inches(0.8)
    section.left_margin = Inches(0.85)
    section.right_margin = Inches(0.85)

    # --- Header & Footer ---
    header = section.header
    hp = header.paragraphs[0]
    hp.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    hrun = hp.add_run("SmallMartApp — Project Technical & Architectural Documentation")
    hrun.font.name = "Calibri"
    hrun.font.size = Pt(8.5)
    hrun.font.color.rgb = RGBColor(0x64, 0x74, 0x8B)

    footer = section.footer
    fp = footer.paragraphs[0]
    fp.alignment = WD_ALIGN_PARAGRAPH.LEFT
    frun = fp.add_run("Institute of Science and Technology Advanced Development (ISTAD) / RUPP — Department of Computer Science")
    frun.font.name = "Calibri"
    frun.font.size = Pt(8.5)
    frun.font.color.rgb = RGBColor(0x64, 0x74, 0x8B)

    # XML Helper Functions
    def set_cell_shading(cell, color_hex):
        shd = parse_xml(f'<w:shd {nsdecls("w")} w:fill="{color_hex}"/>')
        cell._tc.get_or_add_tcPr().append(shd)

    def set_cell_margins(cell, top=120, bottom=120, left=150, right=150):
        tcPr = cell._tc.get_or_add_tcPr()
        tcMar = OxmlElement('w:tcMar')
        for margin_name, val in [('top', top), ('bottom', bottom), ('left', left), ('right', right)]:
            node = OxmlElement(f'w:{margin_name}')
            node.set(qn('w:w'), str(val))
            node.set(qn('w:type'), 'dxa')
            tcMar.append(node)
        tcPr.append(tcMar)

    def set_cell_borders(cell, top=None, bottom=None, left=None, right=None):
        tcPr = cell._tc.get_or_add_tcPr()
        tcBorders = OxmlElement('w:tcBorders')
        borders = {'top': top, 'bottom': bottom, 'left': left, 'right': right}
        for b_name, b_val in borders.items():
            b_el = OxmlElement(f'w:{b_name}')
            if b_val:
                val, sz, color = b_val
                b_el.set(qn('w:val'), val)
                b_el.set(qn('w:sz'), str(sz))
                b_el.set(qn('w:space'), '0')
                b_el.set(qn('w:color'), color)
            else:
                b_el.set(qn('w:val'), 'none')
            tcBorders.append(b_el)
        tcPr.append(tcBorders)

    def add_h1(text):
        p = doc.add_paragraph()
        p.paragraph_format.space_before = Pt(14)
        p.paragraph_format.space_after = Pt(4)
        p.paragraph_format.keep_with_next = True
        run = p.add_run(text)
        run.bold = True
        run.font.name = "Calibri"
        run.font.size = Pt(13)
        run.font.color.rgb = RGBColor(0x0F, 0x17, 0x2A)
        return p

    def add_h2(text):
        p = doc.add_paragraph()
        p.paragraph_format.space_before = Pt(11)
        p.paragraph_format.space_after = Pt(3)
        p.paragraph_format.keep_with_next = True
        run = p.add_run(text)
        run.bold = True
        run.font.name = "Calibri"
        run.font.size = Pt(11)
        run.font.color.rgb = RGBColor(0x1E, 0x29, 0x3B)
        return p

    def add_h3(text):
        p = doc.add_paragraph()
        p.paragraph_format.space_before = Pt(8)
        p.paragraph_format.space_after = Pt(2)
        p.paragraph_format.keep_with_next = True
        run = p.add_run(text)
        run.bold = True
        run.font.name = "Calibri"
        run.font.size = Pt(10)
        run.font.color.rgb = RGBColor(0x1E, 0x29, 0x3B)
        return p

    def add_body(text, space_after=5):
        p = doc.add_paragraph()
        p.paragraph_format.space_before = Pt(0)
        p.paragraph_format.space_after = Pt(space_after)
        p.paragraph_format.line_spacing = 1.15
        p.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
        run = p.add_run(text)
        run.font.name = "Calibri"
        run.font.size = Pt(10)
        run.font.color.rgb = RGBColor(0x33, 0x41, 0x55)
        return p

    def add_bullet(bold_prefix, text, space_after=4):
        p = doc.add_paragraph()
        p.paragraph_format.left_indent = Inches(0.25)
        p.paragraph_format.first_line_indent = Inches(-0.25)
        p.paragraph_format.space_before = Pt(0)
        p.paragraph_format.space_after = Pt(space_after)
        p.paragraph_format.line_spacing = 1.15
        p.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY

        run_b = p.add_run("•  " + bold_prefix + ": ")
        run_b.bold = True
        run_b.font.name = "Calibri"
        run_b.font.size = Pt(9.5)
        run_b.font.color.rgb = RGBColor(0x0F, 0x17, 0x2A)

        run_t = p.add_run(text)
        run_t.font.name = "Calibri"
        run_t.font.size = Pt(9.5)
        run_t.font.color.rgb = RGBColor(0x33, 0x41, 0x55)
        return p

    def add_numbered(num_prefix, bold_title, text, space_after=4):
        p = doc.add_paragraph()
        p.paragraph_format.left_indent = Inches(0.25)
        p.paragraph_format.first_line_indent = Inches(-0.25)
        p.paragraph_format.space_before = Pt(0)
        p.paragraph_format.space_after = Pt(space_after)
        p.paragraph_format.line_spacing = 1.15
        p.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY

        run_b = p.add_run(f"{num_prefix}. {bold_title}: ")
        run_b.bold = True
        run_b.font.name = "Calibri"
        run_b.font.size = Pt(9.5)
        run_b.font.color.rgb = RGBColor(0x0F, 0x17, 0x2A)

        run_t = p.add_run(text)
        run_t.font.name = "Calibri"
        run_t.font.size = Pt(9.5)
        run_t.font.color.rgb = RGBColor(0x33, 0x41, 0x55)
        return p

    def add_code_block(code_lines):
        tbl = doc.add_table(rows=1, cols=1)
        tbl.alignment = WD_TABLE_ALIGNMENT.CENTER
        tbl.autofit = False
        cell = tbl.rows[0].cells[0]
        cell.width = Inches(6.8)
        set_cell_shading(cell, "F8FAFC")
        set_cell_margins(cell, top=120, bottom=120, left=160, right=160)
        set_cell_borders(cell,
                         top=('single', 6, 'CBD5E1'),
                         bottom=('single', 6, 'CBD5E1'),
                         left=('single', 6, 'CBD5E1'),
                         right=('single', 6, 'CBD5E1'))
        
        for i, line in enumerate(code_lines):
            p = cell.paragraphs[0] if i == 0 else cell.add_paragraph()
            p.paragraph_format.space_before = Pt(0)
            p.paragraph_format.space_after = Pt(0)
            p.paragraph_format.line_spacing = 1.15
            run = p.add_run(line)
            run.font.name = "Consolas"
            run.font.size = Pt(8.5)
            run.font.color.rgb = RGBColor(0x1E, 0x29, 0x3B)
            
        sp = doc.add_paragraph()
        sp.paragraph_format.space_before = Pt(0)
        sp.paragraph_format.space_after = Pt(4)
        sp.paragraph_format.line_spacing = 0.5

    def add_callout(heading_text, body_text):
        tbl = doc.add_table(rows=1, cols=1)
        tbl.alignment = WD_TABLE_ALIGNMENT.CENTER
        tbl.autofit = False
        cell = tbl.rows[0].cells[0]
        cell.width = Inches(6.8)
        set_cell_shading(cell, "F0F9FF")
        set_cell_margins(cell, top=140, bottom=140, left=180, right=180)
        set_cell_borders(cell,
                         top=('single', 6, 'BAE6FD'),
                         bottom=('single', 6, 'BAE6FD'),
                         left=('single', 24, '0284C7'),
                         right=('single', 6, 'BAE6FD'))
        
        p1 = cell.paragraphs[0]
        p1.paragraph_format.space_before = Pt(0)
        p1.paragraph_format.space_after = Pt(3)
        r_title = p1.add_run(heading_text)
        r_title.bold = True
        r_title.font.name = "Calibri"
        r_title.font.size = Pt(10)
        r_title.font.color.rgb = RGBColor(0x03, 0x69, 0xA1)

        p2 = cell.add_paragraph()
        p2.paragraph_format.space_before = Pt(0)
        p2.paragraph_format.space_after = Pt(0)
        p2.paragraph_format.line_spacing = 1.15
        r_body = p2.add_run(body_text)
        r_body.font.name = "Calibri"
        r_body.font.size = Pt(9.5)
        r_body.font.color.rgb = RGBColor(0x1E, 0x29, 0x3B)

        sp = doc.add_paragraph()
        sp.paragraph_format.space_before = Pt(0)
        sp.paragraph_format.space_after = Pt(4)
        sp.paragraph_format.line_spacing = 0.5

    # ----------------------------------------------------
    # DOCUMENT CONTENT
    # ----------------------------------------------------

    # Title
    p_title = doc.add_paragraph()
    p_title.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_title.paragraph_format.space_before = Pt(8)
    p_title.paragraph_format.space_after = Pt(4)
    r = p_title.add_run("SmallMartApp: Smart Mart POS & Inventory Management System")
    r.bold = True
    r.font.name = "Calibri"
    r.font.size = Pt(19)
    r.font.color.rgb = RGBColor(0x0F, 0x17, 0x2A)

    # Subtitle
    p_sub = doc.add_paragraph()
    p_sub.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_sub.paragraph_format.space_before = Pt(0)
    p_sub.paragraph_format.space_after = Pt(10)
    r = p_sub.add_run("A Desktop Point of Sale and Inventory Architecture Built with .NET 8 WPF, Clean Architecture, MVVM & KHQR Payments")
    r.italic = True
    r.font.name = "Calibri"
    r.font.size = Pt(10.5)
    r.font.color.rgb = RGBColor(0x33, 0x41, 0x55)

    # Authors
    p_auth = doc.add_paragraph()
    p_auth.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_auth.paragraph_format.space_before = Pt(0)
    p_auth.paragraph_format.space_after = Pt(5)
    r = p_auth.add_run(
        "Seng Porkeat¹, Sim Pengseang¹, Cheng Devith¹, Tourn Vuthy¹, Toch Ratana¹, Va Eric¹,\n"
        "Sorn Sophamarinet¹, Korm TaingAn¹, Leng Senghong¹, Mao Piseth¹, Kay Keo¹, Srorng Sokcheat¹"
    )
    r.bold = True
    r.font.name = "Calibri"
    r.font.size = Pt(9.5)
    r.font.color.rgb = RGBColor(0x0F, 0x17, 0x2A)

    # Affiliation
    p_aff = doc.add_paragraph()
    p_aff.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_aff.paragraph_format.space_before = Pt(0)
    p_aff.paragraph_format.space_after = Pt(3)
    p_aff.paragraph_format.line_spacing = 1.15
    r = p_aff.add_run(
        "¹Institute of Science and Technology Advanced Development (ISTAD) & Royal University of Phnom Penh (RUPP)\n"
        "Faculty of Science, Department of Computer Science — Object-Oriented Analysis & Design (OOAD)\n"
        "#40, St. 273, Sangkat Boeung Kak I, Khan Toul Kork, Phnom Penh, Cambodia"
    )
    r.font.name = "Calibri"
    r.font.size = Pt(8.5)
    r.font.color.rgb = RGBColor(0x47, 0x55, 0x69)

    # Email
    p_email = doc.add_paragraph()
    p_email.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_email.paragraph_format.space_before = Pt(0)
    p_email.paragraph_format.space_after = Pt(12)
    r = p_email.add_run("Email: {pengseangsim210, alexkgm2412, chengdevith5, vuthytoun168, ericva014, ratanatoch68, netsorn7777, kormtaingan99, lengsenghong168, pisethmao2002, keokay888, sokcheatsrorng}@gmail.com")
    r.font.name = "Calibri"
    r.font.size = Pt(8)
    r.font.color.rgb = RGBColor(0x47, 0x55, 0x69)

    # Abstract Box
    tbl_abs = doc.add_table(rows=1, cols=1)
    tbl_abs.alignment = WD_TABLE_ALIGNMENT.CENTER
    tbl_abs.autofit = False
    c_abs = tbl_abs.rows[0].cells[0]
    c_abs.width = Inches(6.8)
    set_cell_shading(c_abs, "F8FAFC")
    set_cell_margins(c_abs, top=140, bottom=140, left=180, right=180)
    set_cell_borders(c_abs,
                     top=('single', 8, 'CBD5E1'),
                     bottom=('single', 8, 'CBD5E1'),
                     left=('single', 8, 'CBD5E1'),
                     right=('single', 8, 'CBD5E1'))

    p_a = c_abs.paragraphs[0]
    p_a.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
    p_a.paragraph_format.space_before = Pt(0)
    p_a.paragraph_format.space_after = Pt(0)
    p_a.paragraph_format.line_spacing = 1.15
    r_ab = p_a.add_run("Abstract — ")
    r_ab.bold = True
    r_ab.font.name = "Calibri"
    r_ab.font.size = Pt(9.5)
    r_ab.font.color.rgb = RGBColor(0x0F, 0x17, 0x2A)

    r_at = p_a.add_run(
        "SmallMartApp is a production-grade, modular Windows desktop Point of Sale (POS) and inventory "
        "management system engineered specifically for convenience stores, mini-marts, and retail kiosks. Built upon Microsoft "
        ".NET 8 and Windows Presentation Foundation (WPF), the platform strictly implements Object-Oriented Analysis and "
        "Design (OOAD) principles, Clean Architecture (Vertical Slice feature separation), and the Model-View-ViewModel (MVVM) "
        "pattern backed by the CommunityToolkit.Mvvm framework. SmallMartApp provides an end-to-end retail solution "
        "spanning real-time barcode scanning, cashier shift cash-drawer auditing, customer loyalty reward accounting, automated "
        "thermal receipt generation, and a fully compliant National Bank of Cambodia (NBC) Bakong KHQR dynamic QR payment "
        "engine. Persistence is abstracted via Microsoft Entity Framework Core 8 with dual support for enterprise Microsoft SQL "
        "Server and embedded SQLite. Decoupled device abstractions allow hardware peripherals (barcode scanners, thermal "
        "receipt printers) to be seamlessly swapped between physical hardware and simulated environments, ensuring maximum "
        "testability, operational stability, and low maintenance overhead."
    )
    r_at.font.name = "Calibri"
    r_at.font.size = Pt(9.5)
    r_at.font.color.rgb = RGBColor(0x33, 0x41, 0x55)

    sp_abs = doc.add_paragraph()
    sp_abs.paragraph_format.space_before = Pt(0)
    sp_abs.paragraph_format.space_after = Pt(6)
    sp_abs.paragraph_format.line_spacing = 0.5

    # 1. Introduction
    add_h1("1. Introduction")
    add_body(
        "In today's fast-paced retail ecosystem, efficient point-of-sale operations and rigorous inventory tracking are "
        "fundamental prerequisites for operational sustainability and customer satisfaction. While large supermarket chains "
        "possess the financial capital to deploy expensive ERP solutions, small-to-medium retail enterprises (SMEs), convenience "
        "stores, and neighborhood marts frequently grapple with fragmented, manual processes. Store operators typically rely on "
        "disjointed spreadsheets, standalone electronic cash registers, or paper logbooks. These outdated methods introduce severe "
        "operational vulnerabilities, including inventory shrinkage, stock-out emergencies, cash register reconciliation discrepancies, "
        "and lengthy customer queues at checkout."
    )
    add_body(
        "Furthermore, the rapid evolution of digital finance across Southeast Asia—specifically the widespread adoption of "
        "Cambodia's national payment gateway (Bakong) and the standardized KHQR code system—has fundamentally shifted consumer "
        "expectations toward frictionless cashless payments. Traditional cash registers fail to bridge physical cash transactions "
        "with dynamic digital QR payments, leaving merchants to manually reconcile separate bank payment notifications against physical receipts."
    )
    add_body(
        "SmallMartApp was conceptualized, designed, and engineered to resolve these challenges comprehensively. By leveraging "
        "modern C# 12 and the .NET 8 desktop runtime, SmallMartApp delivers a high-performance, robust, and visually intuitive retail "
        "management system. It unifies frontline cashier point-of-sale checkout, automated barcode acquisition, customer loyalty "
        "incentives, cashier shift float management, supplier purchase orders, and multi-currency KHQR payment processing into a "
        "single, cohesive desktop application. By adhering strictly to Clean Architecture and MVVM design principles, the software "
        "decouples user interface interactions from business domain logic and database persistence, ensuring enterprise-grade "
        "maintainability, testability, and extensibility."
    )

    # 2. Background & Problem Domain
    add_h1("2. Background & Problem Domain")
    add_body(
        "SmallMartApp was developed as an advanced Object-Oriented Analysis & Design (OOAD) enterprise project by IT researchers "
        "and software engineering students at the Institute of Science and Technology Advanced Development (ISTAD) in collaboration "
        "with the Royal University of Phnom Penh (RUPP). The project's mission is to provide an accessible, rock-solid, and extensible "
        "desktop platform that eliminates retail operational friction for small marts, supermarkets, and grocery kiosks."
    )
    add_body(
        "Through empirical study of local convenience mart operations in Phnom Penh, four critical systemic problems were "
        "identified in existing retail workflows:"
    )

    add_bullet(
        "1. Inventory Shrinkage and Stock Discrepancies",
        "Without automated real-time inventory deductions upon barcode scan, inventory numbers diverge from physical shelf stock. "
        "Merchants frequently experience stock-outs of popular items or over-order perishable goods without automated low-stock warnings."
    )
    add_bullet(
        "2. Cashier Shift Auditing and Financial Gaps",
        "In multi-shift operations (morning, afternoon, night), untracked cash floats lead to untraceable cash shortages. Without "
        "structured shift opening floats, system-expected cash balances, and end-of-shift cash drawer reconciliations, store owners "
        "cannot identify theft or accounting errors."
    )
    add_bullet(
        "3. Checkout Delays and Fragmented Payments",
        "Cashiers are forced to manually enter item amounts or switch between separate bank terminal apps to accept digital payments. "
        "Generating dynamic QR codes with exact transaction amounts and validating cash-change calculations within seconds is essential."
    )
    add_bullet(
        "4. Lack of Customer Loyalty Retention",
        "Small marts struggle to maintain customer loyalty programs without complex hardware. An integrated phone-lookup loyalty system "
        "that automatically accrues points per dollar spent and permits seamless point redemption at checkout provides substantial competitive advantage."
    )

    # Engineering Philosophy Box
    add_callout(
        "📌 SmallMartApp Engineering Philosophy",
        "The core architectural philosophy of SmallMartApp is 'Simplicity on the Surface, Rigor in the Core'. Frontline cashiers benefit "
        "from a fast, touch-friendly, keyboard-accelerated POS interface, while store managers benefit from transparent shift auditing, "
        "stock threshold alerts, and rock-solid relational persistence."
    )

    # 3. System Architecture
    add_h1("3. System Architecture")
    add_body(
        "SmallMartApp is architected following Clean Architecture, Vertical Slice Feature Modularization, and the Model-View-ViewModel (MVVM) "
        "presentation pattern. The solution is structured into three distinct physical layers enforcing strict inward dependency flow, "
        "accompanied by an independent automated testing suite:"
    )

    add_bullet(
        "SmallMartApp.Core (The Domain Brain)",
        "Pure C# class library containing zero external framework dependencies. Encapsulates domain entity models, business enumerations, "
        "service contract interfaces (IProductService, ISalesService, IKhqrService, etc.), hardware driver abstractions (IBarcodeScanner, "
        "IReceiptPrinter), and the generic Result<T> pattern for deterministic error handling."
    )
    add_bullet(
        "SmallMartApp.Infrastructure (Data & Peripherals)",
        "Implements the persistence and hardware contracts defined in Core. Manages Microsoft Entity Framework Core 8 DbContext, SQL Server "
        "and SQLite database providers, relational entity configurations, database seeding, physical/mock hardware drivers, and the EMVCo KHQR "
        "byte generation engine."
    )
    add_bullet(
        "SmallMartApp.UI (WPF Presentation Layer)",
        "Windows Presentation Foundation desktop application containing Views (XAML layout), ViewModels (CommunityToolkit.Mvvm state handlers), "
        "ValueConverters, UI notification services, thermal receipt preview windows, and the DI container bootstrapper (App.xaml.cs)."
    )
    add_bullet(
        "SmallMartApp.Tests (Automated Verification)",
        "xUnit testing project utilizing in-memory database providers to verify sales processing, stock decrements, cash calculations, and "
        "KHQR checksum integrity in continuous integration pipelines."
    )

    # 3.1 Selected Technologies
    add_h2("3.1 Selected Technologies")
    add_body(
        "The following table categorizes the primary technological stack, software tools, frameworks, and packages chosen for SmallMartApp:"
    )

    # Table 1: Selected Technologies
    tech_data = [
        ("Programming Language", "C# 12 (.NET 8.0 SDK)", "Modern, strongly typed language featuring records, pattern matching, nullable references, and high performance."),
        ("UI Presentation Engine", "Windows Presentation Foundation (WPF)", "Hardware-accelerated desktop UI engine with rich XAML declarative markup, DataBinding, and DataTemplates."),
        ("MVVM Framework", "CommunityToolkit.Mvvm (v8.3.2)", "Roslyn source-generator driven framework eliminating boilerplate via [ObservableProperty] and [RelayCommand]."),
        ("Data Persistence (ORM)", "Entity Framework Core 8.0.11", "High-performance object-relational mapper managing database schema, LINQ queries, relationships, and seed data."),
        ("Relational Databases", "Microsoft SQL Server & SQLite", "SQL Server Express for enterprise multi-terminal LAN setups; SQLite for standalone zero-configuration local deployments."),
        ("Dependency Injection", "Microsoft.Extensions.Hosting & DI", "Inversion-of-Control container registering transient business services, singleton mock devices, and view models."),
        ("Digital Payment Engine", "QRCoder (v1.8.0) & Custom KHQR", "Generates high-resolution dynamic QR codes compliant with National Bank of Cambodia (NBC) EMVCo standards."),
        ("Hardware Abstraction", "IBarcodeScanner & IReceiptPrinter", "Decoupled hardware interfaces allowing seamless switching between physical USB POS devices and mock testing drivers."),
        ("Unit & Integration Testing", "xUnit & InMemory EF Core", "Automated test suite verifying checkout rules, cash change mathematics, and stock deduction constraints."),
        ("Development Environments", "Visual Studio 2022 / JetBrains Rider", "Primary IDEs utilized for compilation, XAML visual design, debugging, and solution maintenance.")
    ]

    t1 = doc.add_table(rows=len(tech_data) + 1, cols=3)
    t1.alignment = WD_TABLE_ALIGNMENT.CENTER
    t1.autofit = False

    col_widths1 = [Inches(1.8), Inches(2.2), Inches(2.8)]
    headers1 = ["Component / Category", "Selected Technology", "Description & Architectural Role"]

    # Header Row
    hdr_row1 = t1.rows[0]
    trPr = hdr_row1._tr.get_or_add_trPr()
    trPr.append(parse_xml(f'<w:tblHeader {nsdecls("w")}/>'))
    trPr.append(parse_xml(f'<w:cantSplit {nsdecls("w")}/>'))

    for col_idx, cell in enumerate(hdr_row1.cells):
        cell.width = col_widths1[col_idx]
        set_cell_shading(cell, "1E3A5F")
        set_cell_margins(cell, top=120, bottom=120, left=140, right=140)
        set_cell_borders(cell,
                         top=('single', 6, '0F172A'),
                         bottom=('single', 12, '0F172A'),
                         left=('none', 0, 'auto'),
                         right=('none', 0, 'auto'))
        p = cell.paragraphs[0]
        p.alignment = WD_ALIGN_PARAGRAPH.LEFT
        p.paragraph_format.space_before = Pt(0)
        p.paragraph_format.space_after = Pt(0)
        r = p.add_run(headers1[col_idx])
        r.bold = True
        r.font.name = "Calibri"
        r.font.size = Pt(9.5)
        r.font.color.rgb = RGBColor(0xFF, 0xFF, 0xFF)

    for row_idx, data in enumerate(tech_data):
        row = t1.rows[row_idx + 1]
        trPr = row._tr.get_or_add_trPr()
        trPr.append(parse_xml(f'<w:cantSplit {nsdecls("w")}/>'))
        bg_col = "F8FAFC" if row_idx % 2 == 1 else "FFFFFF"

        for col_idx, cell in enumerate(row.cells):
            cell.width = col_widths1[col_idx]
            set_cell_shading(cell, bg_col)
            set_cell_margins(cell, top=100, bottom=100, left=140, right=140)
            set_cell_borders(cell,
                             top=('single', 4, 'E2E8F0'),
                             bottom=('single', 4, 'E2E8F0'),
                             left=('none', 0, 'auto'),
                             right=('none', 0, 'auto'))
            p = cell.paragraphs[0]
            p.alignment = WD_ALIGN_PARAGRAPH.LEFT
            p.paragraph_format.space_before = Pt(0)
            p.paragraph_format.space_after = Pt(0)
            p.paragraph_format.line_spacing = 1.15
            r = p.add_run(data[col_idx])
            r.font.name = "Calibri"
            r.font.size = Pt(9)
            if col_idx == 0:
                r.bold = True
                r.font.color.rgb = RGBColor(0x0F, 0x17, 0x2A)
            elif col_idx == 1:
                r.font.color.rgb = RGBColor(0x1E, 0x29, 0x3B)
            else:
                r.font.color.rgb = RGBColor(0x33, 0x41, 0x55)

    sp_t1 = doc.add_paragraph()
    sp_t1.paragraph_format.space_before = Pt(0)
    sp_t1.paragraph_format.space_after = Pt(6)
    sp_t1.paragraph_format.line_spacing = 0.5

    # 3.2 Selected Architectural Patterns & Design Principles
    add_h2("3.2 Selected Architectural Patterns & Design Principles")
    add_body(
        "The architecture of SmallMartApp is governed by strict object-oriented design patterns ensuring separation of concerns, "
        "loose coupling, and testability:"
    )

    add_h3("A. The MVVM (Model-View-ViewModel) Triad:")
    add_body(
        "In contrast to traditional Windows Forms where business logic, SQL commands, and UI control manipulations are mixed within "
        "code-behind files, SmallMartApp strictly enforces the MVVM architectural separation:"
    )

    # Table 2: MVVM Layers
    mvvm_data = [
        ("Model", "Zero UI Knowledge", "Pure POCO classes representing database entities and domain rules. No references to WPF controls or buttons.", "Product.cs, Sale.cs, CashierShift.cs"),
        ("View", "Visual Layout Only", "Declarative XAML definitions specifying margins, colors, controls, data templates, and bindings. Zero business logic.", "PosCheckoutView.xaml, DashboardView.xaml"),
        ("ViewModel", "100% UI Logic", "Maintains UI state, handles button commands via [RelayCommand], executes validations, and coordinates service calls.", "PosCheckoutViewModel.cs, DashboardViewModel.cs")
    ]

    t2 = doc.add_table(rows=len(mvvm_data) + 1, cols=4)
    t2.alignment = WD_TABLE_ALIGNMENT.CENTER
    t2.autofit = False

    col_widths2 = [Inches(1.1), Inches(1.3), Inches(2.6), Inches(1.8)]
    headers2 = ["Layer", "UI Interactivity", "Core Contents & Responsibilities", "Project Implementation Example"]

    hdr_row2 = t2.rows[0]
    trPr2 = hdr_row2._tr.get_or_add_trPr()
    trPr2.append(parse_xml(f'<w:tblHeader {nsdecls("w")}/>'))
    trPr2.append(parse_xml(f'<w:cantSplit {nsdecls("w")}/>'))

    for col_idx, cell in enumerate(hdr_row2.cells):
        cell.width = col_widths2[col_idx]
        set_cell_shading(cell, "1E3A5F")
        set_cell_margins(cell, top=120, bottom=120, left=130, right=130)
        set_cell_borders(cell,
                         top=('single', 6, '0F172A'),
                         bottom=('single', 12, '0F172A'),
                         left=('none', 0, 'auto'),
                         right=('none', 0, 'auto'))
        p = cell.paragraphs[0]
        p.alignment = WD_ALIGN_PARAGRAPH.LEFT
        p.paragraph_format.space_before = Pt(0)
        p.paragraph_format.space_after = Pt(0)
        r = p.add_run(headers2[col_idx])
        r.bold = True
        r.font.name = "Calibri"
        r.font.size = Pt(9.5)
        r.font.color.rgb = RGBColor(0xFF, 0xFF, 0xFF)

    for row_idx, data in enumerate(mvvm_data):
        row = t2.rows[row_idx + 1]
        trPr = row._tr.get_or_add_trPr()
        trPr.append(parse_xml(f'<w:cantSplit {nsdecls("w")}/>'))
        bg_col = "F8FAFC" if row_idx % 2 == 1 else "FFFFFF"

        for col_idx, cell in enumerate(row.cells):
            cell.width = col_widths2[col_idx]
            set_cell_shading(cell, bg_col)
            set_cell_margins(cell, top=100, bottom=100, left=130, right=130)
            set_cell_borders(cell,
                             top=('single', 4, 'E2E8F0'),
                             bottom=('single', 4, 'E2E8F0'),
                             left=('none', 0, 'auto'),
                             right=('none', 0, 'auto'))
            p = cell.paragraphs[0]
            p.alignment = WD_ALIGN_PARAGRAPH.LEFT
            p.paragraph_format.space_before = Pt(0)
            p.paragraph_format.space_after = Pt(0)
            p.paragraph_format.line_spacing = 1.15
            r = p.add_run(data[col_idx])
            r.font.name = "Calibri"
            r.font.size = Pt(9)
            if col_idx == 0:
                r.bold = True
                r.font.color.rgb = RGBColor(0x0F, 0x17, 0x2A)
            elif col_idx == 3:
                r.font.name = "Consolas"
                r.font.size = Pt(8.5)
                r.font.color.rgb = RGBColor(0x0F, 0x17, 0x2A)
            else:
                r.font.color.rgb = RGBColor(0x33, 0x41, 0x55)

    sp_t2 = doc.add_paragraph()
    sp_t2.paragraph_format.space_before = Pt(0)
    sp_t2.paragraph_format.space_after = Pt(6)
    sp_t2.paragraph_format.line_spacing = 0.5

    add_h3("B. Modern Two-Way Data Binding and Relay Commands:")
    add_body(
        "Rather than accessing UI controls by name (e.g., txtBarcode.Text), SmallMartApp utilizes Roslyn source generators from "
        "CommunityToolkit.Mvvm. Backing fields decorated with [ObservableProperty] automatically synthesize INotifyPropertyChanged events, "
        "allowing XAML controls to update instantly upon property modification. Methods decorated with [RelayCommand] automatically "
        "generate asynchronous ICommand properties with built-in execution state tracking and button disablement."
    )

    add_code_block([
        "// Exemplary ViewModel Implementation in SmallMartApp",
        "[ObservableProperty]",
        "private string _barcodeInput = string.Empty;",
        "",
        "[ObservableProperty]",
        "private ObservableCollection<CartItemViewModel> _cartItems = new();",
        "",
        "[RelayCommand(CanExecute = nameof(CanCompleteCheckout))]",
        "private async Task CompleteCheckoutAsync()",
        "{",
        "    var result = await _salesService.ProcessSaleAsync(cartData, CashReceived, CurrentShiftId);",
        "    if (result.IsSuccess) { StatusMessage = \"Sale completed!\"; ResetCart(); }",
        "}"
    ])

    add_h3("C. Decoupled View Navigation via DataTemplates:")
    add_body(
        "SmallMartApp eliminates hard-coded navigation logic between screens. The main application window hosts a single ContentControl "
        "bound to MainViewModel.CurrentView. In App.xaml, global DataTemplates map ViewModel types directly to View UserControls:"
    )

    add_code_block([
        '<DataTemplate DataType="{x:Type vm:PosCheckoutViewModel}">',
        '    <views:PosCheckoutView />',
        '</DataTemplate>'
    ])

    add_body(
        "When the user clicks a navigation item (e.g., POS, Inventory, Shift, Customers), the MainViewModel simply sets CurrentView to the "
        "appropriate ViewModel instance. WPF automatically resolves the visual blueprint, keeping ViewModels 100% free of UI references."
    )

    # 3.3 ERD & Database Design
    add_h2("3.3 Entity Relationship Diagram (ERD) & Database Design")
    add_body(
        "SmallMartApp's relational database schema is modeled to ensure strict referential integrity, normalized relations, and high query "
        "performance. The schema consists of 10 primary relational tables configured using EF Core Fluent API:"
    )

    # ERD Diagram Box
    erd_diagram = [
        "                               ┌─────────────┐",
        "                               │  Category   │",
        "                               └──────┬──────┘",
        "                                      │ 1",
        "                                      │ * (CategoryId)",
        "┌──────────────┐ 1          * ┌──────┴──────┐ * (CustomerId) 1 ┌──────────────┐",
        "│   Supplier   ├─────────────►│   Product   │◄─────────────────┤   Customer   │",
        "└──────┬───────┘ (SupplierId) └──────┬──────┘                  └──────┬───────┘",
        "       │ 1                           │ 1                              │ 1",
        "       │ *                           │ *                              │ *",
        "┌──────┴───────┐              ┌──────┴──────┐ 1             *  ┌──────┴───────┐",
        "│PurchaseOrder │              │  SaleItem   ├─────────────────►│     Sale     │",
        "└──────┬───────┘              └─────────────┘   (SaleId)       └──────┬───────┘",
        "       │ 1                                                            │ *",
        "       │ *                                                            │ 1 (ShiftId)",
        "┌──────┴──────────┐                                            ┌──────┴───────┐",
        "│PurchaseOrderItem│                                            │ CashierShift │",
        "└─────────────────┘                                            └──────┬───────┘",
        "                                                                      │ *",
        "                                                                      │ 1 (UserId)",
        "                                                               ┌──────┴───────┐",
        "                                                               │     User     │",
        "                                                               └──────────────┘"
    ]
    add_code_block(erd_diagram)

    # Table 3: Database Schema (10 tables)
    schema_data = [
        ("Categories", "Id (int, PK)", "None", "Name (VARCHAR 100, Required), Description (VARCHAR 250), CreatedAt"),
        ("Suppliers", "Id (int, PK)", "None", "CompanyName (VARCHAR 150), PhoneNumber (VARCHAR 30), ContactPerson, Address"),
        ("Products", "Id (int, PK)", "CategoryId (FK)\nSupplierId (FK)", "Barcode (VARCHAR 50, UNIQUE Index), Name (VARCHAR 150), CostPrice (18,2), SellPrice (18,2), StockQuantity (int), LowStockAlertThreshold (int), IsActive (bool)"),
        ("Customers", "Id (int, PK)", "None", "PhoneNumber (VARCHAR 30, UNIQUE Index), FullName (VARCHAR 120), Points (int, Loyalty Accrual)"),
        ("Users", "Id (int, PK)", "None", "Username (VARCHAR 50, UNIQUE Index), FullName, WorkingShift, PasswordHash, Role (Admin, Cashier), IsActive"),
        ("CashierShifts", "Id (int, PK)", "UserId (FK)", "StartingCash (18,2), ExpectedCash (18,2), ActualCash (18,2), Status (Open, Closed), StartedAt, EndedAt"),
        ("Sales", "Id (int, PK)", "ShiftId (FK)\nCustomerId (FK)", "ReceiptNumber (VARCHAR 50, Unique), TotalAmount (18,2), DiscountAmount (18,2), CashReceived (18,2), ChangeGiven (18,2), PaymentMethod (Cash, KHQR, Card), CreatedAt"),
        ("SaleItems", "Id (int, PK)", "SaleId (FK, Cascade)\nProductId (FK)", "ProductName (VARCHAR 150), UnitPrice (18,2), Quantity (int)"),
        ("PurchaseOrders", "Id (int, PK)", "SupplierId (FK)", "TotalCost (18,2), Status (Pending, Completed), OrderDate (DateTime)"),
        ("PurchaseOrderItems", "Id (int, PK)", "PurchaseOrderId (FK, Cascade)\nProductId (FK)", "ProductName (VARCHAR 150), UnitCost (18,2), Quantity (int)")
    ]

    t3 = doc.add_table(rows=len(schema_data) + 1, cols=4)
    t3.alignment = WD_TABLE_ALIGNMENT.CENTER
    t3.autofit = False

    col_widths3 = [Inches(1.4), Inches(1.0), Inches(1.5), Inches(2.9)]
    headers3 = ["Table Name", "Primary Key", "Foreign Keys", "Key Attributes & Constraints"]

    hdr_row3 = t3.rows[0]
    trPr3 = hdr_row3._tr.get_or_add_trPr()
    trPr3.append(parse_xml(f'<w:tblHeader {nsdecls("w")}/>'))
    trPr3.append(parse_xml(f'<w:cantSplit {nsdecls("w")}/>'))

    for col_idx, cell in enumerate(hdr_row3.cells):
        cell.width = col_widths3[col_idx]
        set_cell_shading(cell, "1E3A5F")
        set_cell_margins(cell, top=120, bottom=120, left=130, right=130)
        set_cell_borders(cell,
                         top=('single', 6, '0F172A'),
                         bottom=('single', 12, '0F172A'),
                         left=('none', 0, 'auto'),
                         right=('none', 0, 'auto'))
        p = cell.paragraphs[0]
        p.alignment = WD_ALIGN_PARAGRAPH.LEFT
        p.paragraph_format.space_before = Pt(0)
        p.paragraph_format.space_after = Pt(0)
        r = p.add_run(headers3[col_idx])
        r.bold = True
        r.font.name = "Calibri"
        r.font.size = Pt(9.5)
        r.font.color.rgb = RGBColor(0xFF, 0xFF, 0xFF)

    for row_idx, data in enumerate(schema_data):
        row = t3.rows[row_idx + 1]
        trPr = row._tr.get_or_add_trPr()
        trPr.append(parse_xml(f'<w:cantSplit {nsdecls("w")}/>'))
        bg_col = "F8FAFC" if row_idx % 2 == 1 else "FFFFFF"

        for col_idx, cell in enumerate(row.cells):
            cell.width = col_widths3[col_idx]
            set_cell_shading(cell, bg_col)
            set_cell_margins(cell, top=100, bottom=100, left=130, right=130)
            set_cell_borders(cell,
                             top=('single', 4, 'E2E8F0'),
                             bottom=('single', 4, 'E2E8F0'),
                             left=('none', 0, 'auto'),
                             right=('none', 0, 'auto'))
            p = cell.paragraphs[0]
            p.alignment = WD_ALIGN_PARAGRAPH.LEFT
            p.paragraph_format.space_before = Pt(0)
            p.paragraph_format.space_after = Pt(0)
            p.paragraph_format.line_spacing = 1.15
            r = p.add_run(data[col_idx])
            r.font.name = "Calibri"
            r.font.size = Pt(8.5)
            if col_idx == 0:
                r.bold = True
                r.font.color.rgb = RGBColor(0x0F, 0x17, 0x2A)
            elif col_idx == 1:
                r.font.color.rgb = RGBColor(0x1E, 0x29, 0x3B)
            elif col_idx == 2:
                r.font.color.rgb = RGBColor(0x47, 0x55, 0x69)
            else:
                r.font.color.rgb = RGBColor(0x33, 0x41, 0x55)

    sp_t3 = doc.add_paragraph()
    sp_t3.paragraph_format.space_before = Pt(0)
    sp_t3.paragraph_format.space_after = Pt(6)
    sp_t3.paragraph_format.line_spacing = 0.5

    # 3.4 Core Subsystems & Business Workflow Details
    add_h2("3.4 Core Subsystems & Business Workflow Details")

    add_h3("A. Point of Sale (POS) Checkout & Transaction Flow:")
    add_body(
        "The checkout process coordinates the interaction between physical barcode input, cart item accumulation, discount and "
        "loyalty calculations, stock deduction, and receipt printing. When a sale is confirmed, the SalesService executes within "
        "an atomic database transaction: validating item stock availability, decrementing physical inventory, computing change given, "
        "accruing customer loyalty points (1 point per dollar spent), logging the transaction to the active cashier shift, and dispatching "
        "formatted thermal receipt data to the printer driver."
    )

    add_h3("B. Cashier Shift & Cash Float Reconciliation:")
    add_body(
        "To prevent cash leakage and ensure accountability, cashiers must initiate an official shift by entering an opening cash float "
        "(StartingCash). Throughout the shift, all cash checkout transactions automatically increment the ExpectedCash balance. Upon shift "
        "termination, the cashier physically counts the cash drawer and submits ActualCash. The system calculates variance: Discrepancy = "
        "ActualCash - ExpectedCash. This audit trail is permanently bound to the cashier user record."
    )

    add_h3("C. Cambodian National Bank (NBC) KHQR Payment Engine:")
    add_body(
        "SmallMartApp integrates a custom KHQR generator compliant with the EMVCo standard mandated by the National Bank of Cambodia (NBC). "
        "The engine dynamically formats Tag-Length-Value (TLV) payloads supporting both standard NBC Bakong accounts (e.g. user@aba) and "
        "ACLEDA Bank EMVCo routing configurations (Tag 29 sub-tags 00, 01, 02 with Tag 39 dual currency 2CCY indicators). Payloads are "
        "validated with a 16-bit CRC-CCITT (polynomial 0x1021, initial 0xFFFF) checksum and rendered directly into QR Code PNG images on the cashier screen."
    )

    # 4. Evaluation
    add_h1("4. Evaluation")
    add_body(
        "SmallMartApp was evaluated through rigorous simulated retail operations, multi-item checkout stress tests, and automated unit test "
        "suites. The evaluation demonstrates significant operational advantages alongside distinct technical trade-offs."
    )

    add_h2("4.1 Project Strengths")
    add_bullet(
        "Architectural Rigor & Clean Separation",
        "Strict adherence to Clean Architecture and MVVM guarantees that business rules (SalesService, Stock deductions) are completely "
        "independent of presentation controls. The solution can be refactored, extended, or migrated to web/mobile APIs without rewriting domain logic."
    )
    add_bullet(
        "Modern Cashless Payment Integration (KHQR)",
        "Native generation of EMVCo-compliant KHQR dynamic codes eliminates human cashier input error when entering transaction amounts, "
        "drastically accelerating checkout speed for mobile banking customers across Cambodia."
    )
    add_bullet(
        "Real-Time Shift Auditing & Cash Accountability",
        "The shift float tracking engine prevents cashier cash discrepancies, providing store owners with clear, auditable logs of expected "
        "versus actual cash amounts per shift."
    )
    add_bullet(
        "Decoupled Hardware Driver Layer",
        "By utilizing IBarcodeScanner and IReceiptPrinter interfaces, the software runs seamlessly in mock development mode without physical "
        "POS peripherals, while instantly binding to real USB laser scanners and thermal ESC/POS printers in production."
    )
    add_bullet(
        "Dual Database Persistence Flexibility",
        "EF Core configuration enables zero-configuration SQLite for single kiosk stores, or Microsoft SQL Server for multi-lane grocery marts."
    )
    add_bullet(
        "Automated Test Verification",
        "Comprehensive xUnit test coverage validates critical checkout math, stock decrement thresholds, and KHQR string checksum generation."
    )

    add_h2("4.2 Project Weaknesses & Limitations")
    add_bullet(
        "Windows Operating System Dependency",
        "Because the user interface is constructed using Windows Presentation Foundation (WPF), SmallMartApp is constrained to Microsoft "
        "Windows environments (Windows 10/11). Cross-platform deployment (macOS, Linux, Android) would require a future migration to .NET MAUI or Avalonia."
    )
    add_bullet(
        "Localized Persistence Without Real-Time Cloud Sync",
        "The current release operates on local area networks (LAN) or single machines. Multi-branch supermarkets operating across different "
        "cities require a centralized cloud synchronization service to aggregate inventory and sales data in real time."
    )
    add_bullet(
        "Lack of Direct Serial ESC/POS Hardware Drivers",
        "Receipt printing is currently handled via WPF document drawing and text file logging. Future releases should implement raw ESC/POS "
        "binary command transmission via Serial/COM and TCP/IP sockets for ultra-fast thermal roll cutting and cash drawer ejection."
    )
    add_bullet(
        "Reporting Export Formats",
        "While real-time dashboard KPIs are displayed on screen, the system currently lacks built-in export modules to generate downloadable "
        "Excel (.xlsx) or PDF financial statements for store accounting."
    )

    # 5. Conclusion & Future Work
    add_h1("5. Conclusion & Future Work")
    add_body(
        "SmallMartApp demonstrates the successful application of modern Object-Oriented Analysis & Design (OOAD) principles, Clean Architecture, "
        "and WPF MVVM patterns to real-world retail automation. By addressing the critical pain points of inventory shrinkage, cash drawer "
        "discrepancies, and fragmented digital payment methods, SmallMartApp provides small-to-medium retail stores with an enterprise-caliber "
        "point of sale and inventory management solution. The integration of Cambodia's national KHQR standard bridges traditional physical "
        "retail with modern cashless convenience, offering a streamlined experience for cashiers, customers, and store managers alike."
    )
    add_body(
        "Future development milestones for SmallMartApp include:"
    )

    add_numbered("1", "Cloud API Gateway & Synchronization", "Implementing an ASP.NET Core Web API backend with Azure SQL to facilitate multi-branch inventory transfers and centralized reporting.")
    add_numbered("2", "Cross-Platform Mobile Management App", "Building a companion mobile application using .NET MAUI allowing store owners to view live sales KPIs, low-stock notifications, and shift closures from smartphones.")
    add_numbered("3", "Predictive Restocking & Analytics", "Incorporating machine learning demand forecasting to predict stock reorder dates based on historical sales trends.")

    # References
    add_h1("References")
    refs = [
        ("[1]", "Microsoft Corporation. (2024). Windows Presentation Foundation (WPF) .NET 8 Documentation. https://learn.microsoft.com/en-us/dotnet/desktop/wpf/"),
        ("[2]", "Microsoft Community Toolkit. (2024). MVVM Toolkit Introduction & Architecture Guidelines. https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/"),
        ("[3]", "Microsoft Corporation. (2024). Entity Framework Core 8 Overview & Best Practices. https://learn.microsoft.com/en-us/ef/core/"),
        ("[4]", "National Bank of Cambodia (NBC). (2022). KHQR Technical Specification & Bakong EMVCo Standard. Phnom Penh, Cambodia."),
        ("[5]", "Martin, R. C. (2017). Clean Architecture: A Craftsman's Guide to Software Structure and Design. Prentice Hall."),
        ("[6]", "Gamma, E., Helm, R., Johnson, R., & Vlissides, J. (1994). Design Patterns: Elements of Reusable Object-Oriented Software. Addison-Wesley."),
        ("[7]", "EMVCo, LLC. (2020). EMV QR Code Specification for Payment Systems (EMVCo QRCPS). https://www.emvco.com/")
    ]

    for tag, text in refs:
        p_ref = doc.add_paragraph()
        p_ref.paragraph_format.left_indent = Inches(0.4)
        p_ref.paragraph_format.first_line_indent = Inches(-0.4)
        p_ref.paragraph_format.space_before = Pt(0)
        p_ref.paragraph_format.space_after = Pt(3)
        p_ref.paragraph_format.line_spacing = 1.15
        
        rtag = p_ref.add_run(tag + " ")
        rtag.bold = True
        rtag.font.name = "Calibri"
        rtag.font.size = Pt(9)
        rtag.font.color.rgb = RGBColor(0x0F, 0x17, 0x2A)

        rtext = p_ref.add_run(text)
        rtext.font.name = "Calibri"
        rtext.font.size = Pt(9)
        rtext.font.color.rgb = RGBColor(0x33, 0x41, 0x55)

    # Save to both paths
    dest1 = r"d:\RUPPClass\year4\OOAD\project\SmallMartApp\docs\SmallMartApp_Documentation.docx"
    dest2 = r"d:\RUPPClass\year4\OOAD\project\SmallMartApp\SmallMartApp_Documentation.docx"
    doc.save(dest1)
    doc.save(dest2)
    print(f"Successfully saved to:\n  1. {dest1}\n  2. {dest2}")

if __name__ == "__main__":
    create_document()
