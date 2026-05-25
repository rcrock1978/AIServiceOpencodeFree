using Dapper;
using Npgsql;

namespace CatalogService.Services;

public class SeedService
{
    private readonly string _connectionString;

    public SeedService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("PgVector")
            ?? throw new InvalidOperationException("Connection string 'PgVector' not found.");
    }

    public async Task<int> SeedAsync()
    {
        using var conn = new NpgsqlConnection(_connectionString);

        var products = new List<(string Name, string Description, decimal Price, string Brand, string Type, string ImageUrl, decimal Rating, int Stock)>
        {
            ("iPhone 16 Pro Max", "Apple's flagship smartphone with A18 Pro chip, 48MP camera system, and titanium design.", 1199.99m, "Apple", "Phone", "/images/iphone16promax.jpg", 4.8m, 45),
            ("iPhone 16 Pro", "Professional-grade smartphone with advanced camera capabilities.", 999.99m, "Apple", "Phone", "/images/iphone16pro.jpg", 4.7m, 60),
            ("iPhone 16", "Powerful and accessible iPhone with A18 chip.", 799.99m, "Apple", "Phone", "/images/iphone16.jpg", 4.6m, 80),
            ("iPhone 15", "Previous generation iPhone with exceptional performance.", 699.99m, "Apple", "Phone", "/images/iphone15.jpg", 4.5m, 35),
            ("Samsung Galaxy S25 Ultra", "Premium Android smartphone with S Pen and 200MP camera.", 1299.99m, "Samsung", "Phone", "/images/galaxys25ultra.jpg", 4.7m, 40),
            ("Samsung Galaxy S25+", "Large-screen flagship with advanced AI features.", 999.99m, "Samsung", "Phone", "/images/galaxys25plus.jpg", 4.6m, 55),
            ("Samsung Galaxy S25", "Compact flagship with Galaxy AI capabilities.", 799.99m, "Samsung", "Phone", "/images/galaxys25.jpg", 4.5m, 70),
            ("Google Pixel 9 Pro", "Google's flagship with Tensor G4 chip and best-in-class camera.", 899.99m, "Google", "Phone", "/images/pixel9pro.jpg", 4.6m, 30),
            ("Google Pixel 9", "Smartphone with AI-powered features and excellent camera.", 699.99m, "Google", "Phone", "/images/pixel9.jpg", 4.5m, 45),
            ("OnePlus 13", "Flagship killer with Snapdragon 8 Gen 4 and 100W charging.", 749.99m, "OnePlus", "Phone", "/images/oneplus13.jpg", 4.4m, 25),
            ("MacBook Pro 16 M4 Max", "Apple's most powerful laptop with M4 Max chip and 48GB unified memory.", 3499.99m, "Apple", "Laptop", "/images/macbookpro16.jpg", 4.9m, 20),
            ("MacBook Pro 14 M4 Pro", "Professional laptop with M4 Pro chip and Liquid Retina XDR display.", 1999.99m, "Apple", "Laptop", "/images/macbookpro14.jpg", 4.8m, 30),
            ("MacBook Air M4", "Ultra-thin laptop with M4 chip and all-day battery life.", 1099.99m, "Apple", "Laptop", "/images/macbookairm4.jpg", 4.7m, 50),
            ("Dell XPS 16", "Premium Windows laptop with Intel Core Ultra and OLED display.", 2199.99m, "Dell", "Laptop", "/images/dellxps16.jpg", 4.6m, 25),
            ("Dell XPS 14", "Compact premium laptop with stunning display.", 1599.99m, "Dell", "Laptop", "/images/dellxps14.jpg", 4.5m, 35),
            ("Dell Inspiron 16", "Versatile laptop for productivity and entertainment.", 899.99m, "Dell", "Laptop", "/images/dellinspiron16.jpg", 4.3m, 60),
            ("Lenovo ThinkPad X1 Carbon Gen 12", "Ultralight business laptop with Intel Core Ultra 7.", 1849.99m, "Lenovo", "Laptop", "/images/thinkpadx1c12.jpg", 4.7m, 20),
            ("Lenovo Yoga 9i", "Convertible laptop with 360° hinge and premium audio.", 1399.99m, "Lenovo", "Laptop", "/images/yoga9i.jpg", 4.5m, 30),
            ("HP Spectre x360 16", "Premium 2-in-1 laptop with OLED touch display.", 1699.99m, "HP", "Laptop", "/images/spectrex360.jpg", 4.5m, 25),
            ("ASUS ROG Zephyrus G16", "Gaming laptop with RTX 4070 and 165Hz display.", 1999.99m, "ASUS", "Laptop", "/images/rogzephyrusg16.jpg", 4.6m, 15),
            ("iPad Pro M4 13", "Apple's most advanced tablet with M4 chip and Ultra Retina XDR display.", 1299.99m, "Apple", "Tablet", "/images/ipadprom4.jpg", 4.8m, 35),
            ("iPad Pro M4 11", "Powerful tablet with M4 chip in a compact form factor.", 999.99m, "Apple", "Tablet", "/images/ipadpro11m4.jpg", 4.7m, 40),
            ("iPad Air M3", "Versatile tablet with M3 chip and Liquid Retina display.", 599.99m, "Apple", "Tablet", "/images/ipadairm3.jpg", 4.6m, 55),
            ("iPad 10th Gen", "Everyday tablet with A14 Bionic chip.", 349.99m, "Apple", "Tablet", "/images/ipad10.jpg", 4.5m, 70),
            ("Samsung Galaxy Tab S10 Ultra", "Premium Android tablet with 14.6 Dynamic AMOLED display.", 1199.99m, "Samsung", "Tablet", "/images/galaxytabs10ultra.jpg", 4.6m, 25),
            ("Samsung Galaxy Tab S10+", "Large-screen tablet with included S Pen.", 899.99m, "Samsung", "Tablet", "/images/galaxytabs10plus.jpg", 4.5m, 35),
            ("Microsoft Surface Pro 11", "Windows tablet with Snapdragon X Elite and Copilot+ features.", 1499.99m, "Microsoft", "Tablet", "/images/surfacepro11.jpg", 4.5m, 20),
            ("Amazon Fire Max 11", "Affordable tablet for entertainment and productivity.", 249.99m, "Amazon", "Tablet", "/images/firemax11.jpg", 4.2m, 80),
            ("Apple Watch Ultra 3", "Rugged smartwatch for extreme sports with titanium case.", 899.99m, "Apple", "Watch", "/images/watchultra3.jpg", 4.8m, 25),
            ("Apple Watch Series 10", "Advanced health monitoring smartwatch with always-on display.", 499.99m, "Apple", "Watch", "/images/watchseries10.jpg", 4.7m, 50),
            ("Apple Watch SE 3", "Essential smartwatch at an affordable price.", 279.99m, "Apple", "Watch", "/images/watchse3.jpg", 4.5m, 65),
            ("Samsung Galaxy Watch 7 Ultra", "Premium smartwatch with titanium grade and extended battery.", 699.99m, "Samsung", "Watch", "/images/galaxywatch7ultra.jpg", 4.6m, 30),
            ("Samsung Galaxy Watch 7", "Feature-packed smartwatch with BioActive sensor.", 399.99m, "Samsung", "Watch", "/images/galaxywatch7.jpg", 4.5m, 45),
            ("Garmin Fenix 8", "Multisport GPS watch with AMOLED display and mapping.", 999.99m, "Garmin", "Watch", "/images/fenix8.jpg", 4.7m, 15),
            ("Garmin Forerunner 965", "Premium running watch with GPS and training metrics.", 599.99m, "Garmin", "Watch", "/images/forerunner965.jpg", 4.6m, 20),
            ("AirPods Max 2", "Over-ear headphones with exceptional sound and active noise cancellation.", 549.99m, "Apple", "Headphones", "/images/airpodsmax2.jpg", 4.7m, 30),
            ("AirPods Pro 3", "Wireless earbuds with adaptive audio and hearing health features.", 249.99m, "Apple", "Headphones", "/images/airpodspro3.jpg", 4.7m, 60),
            ("AirPods 4", "Open-ear design with spatial audio and comfortable fit.", 129.99m, "Apple", "Headphones", "/images/airpods4.jpg", 4.5m, 80),
            ("Sony WH-1000XM6", "Industry-leading noise canceling headphones with exceptional sound.", 399.99m, "Sony", "Headphones", "/images/wh1000xm6.jpg", 4.8m, 35),
            ("Sony WF-1000XM6", "Premium wireless earbuds with world-class noise cancellation.", 299.99m, "Sony", "Headphones", "/images/wf1000xm6.jpg", 4.7m, 40),
            ("Bose QuietComfort Ultra", "Immersive audio with spatial noise cancellation technology.", 429.99m, "Bose", "Headphones", "/images/qcultra.jpg", 4.7m, 25),
            ("Bose QuietComfort Earbuds Ultra", "Best-in-class noise cancelling earbuds with spatial audio.", 299.99m, "Bose", "Headphones", "/images/qcearbudsultra.jpg", 4.6m, 30),
            ("Samsung Galaxy Buds3 Pro", "Premium earbuds with dual amplifiers and blade design.", 249.99m, "Samsung", "Headphones", "/images/galaxybuds3pro.jpg", 4.5m, 45),
            ("Sennheiser Momentum 4", "High-fidelity wireless headphones with adaptive noise cancellation.", 349.99m, "Sennheiser", "Headphones", "/images/momentum4.jpg", 4.6m, 20),
            ("Sony A7 V", "Full-frame mirrorless camera with 61MP sensor and AI autofocus.", 3999.99m, "Sony", "Camera", "/images/sonya7v.jpg", 4.8m, 10),
            ("Sony A7R V", "High-resolution mirrorless camera with 61MP sensor.", 3499.99m, "Sony", "Camera", "/images/sonya7rv.jpg", 4.7m, 12),
            ("Canon EOS R5 Mark II", "Professional mirrorless camera with 45MP sensor and 8K video.", 4299.99m, "Canon", "Camera", "/images/eosr5ii.jpg", 4.8m, 8),
            ("Canon EOS R6 Mark III", "Versatile full-frame camera with 24MP sensor and 4K video.", 2499.99m, "Canon", "Camera", "/images/eosr6iii.jpg", 4.6m, 15),
            ("Nintendo Switch 2", "Next-generation gaming console with enhanced graphics and performance.", 449.99m, "Nintendo", "Gaming", "/images/switch2.jpg", 4.7m, 50),
            ("PlayStation 5 Pro", "Sony's most powerful console with enhanced ray tracing and 8K output.", 699.99m, "Sony", "Gaming", "/images/ps5pro.jpg", 4.8m, 30),
            ("Xbox Series X 2TB", "Microsoft's most powerful console with 2TB SSD.", 599.99m, "Microsoft", "Gaming", "/images/xboxseriesx.jpg", 4.6m, 35),
            ("Apple TV 4K 3rd Gen", "Streaming device with A15 chip and HDR10+ support.", 149.99m, "Apple", "Streaming", "/images/appletv4k.jpg", 4.6m, 40),
            ("Sony Bravia XR A95L 65", "65 QD-OLED TV with Cognitive Processor XR.", 2799.99m, "Sony", "TV", "/images/braviaa95l.jpg", 4.8m, 10),
            ("Samsung Neo QLED 8K 75", "75 8K Neo QLED TV with Quantum Matrix Technology.", 4999.99m, "Samsung", "TV", "/images/neosamsung8k.jpg", 4.7m, 5),
        };

        foreach (var p in products)
        {
            var sql = @"
                INSERT INTO products (id, name, description, price, brand, type, image_url, rating, stock, created_at, updated_at)
                VALUES (@Id, @Name, @Description, @Price, @Brand, @Type, @ImageUrl, @Rating, @Stock, @CreatedAt, @UpdatedAt)
                ON CONFLICT (id) DO NOTHING";

            await conn.ExecuteAsync(sql, new
            {
                Id = Guid.NewGuid(),
                p.Name,
                p.Description,
                p.Price,
                p.Brand,
                p.Type,
                p.ImageUrl,
                p.Rating,
                p.Stock,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        return products.Count;
    }
}
