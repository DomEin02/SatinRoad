using System.ComponentModel.DataAnnotations;
using API.Dtos;
using Infa;
using LinqToDB;

namespace Tests;

public class CategoriesControllerTests
{
    public class GetAllTests : ApiTest
    {
        [Fact]
        public void Returns_all_categories_ordered_by_name()
        {
            // Indsæt dem i "forkert" rækkefølge, for at bevise at sorteringen faktisk virker.
            Db.Insert(new Category { Id = "1", Name = "Weaponry" });
            Db.Insert(new Category { Id = "2", Name = "Drugs" });

            var result = CategoriesController.GetAll();

            // Forventer alfabetisk rækkefølge: Drugs før Weaponry.
            Assert.Equal(["Drugs", "Weaponry"], result.Select(c => c.Name));
        }
    }

    public class CreateTests : ApiTest
    {
        [Fact]
        public void Inserts_a_new_category()
        {
            var created = CategoriesController.Create(new CategoryCreateRequest("Stolen Artifacts"));

            // Tjek at svaret indeholder det rigtige navn...
            Assert.Equal("Stolen Artifacts", created.Name);
            // og at den rent faktisk blev gemt i databasen (kun 1 kategori findes nu).
            Assert.Single(Db.Categories().ToList());
        }

        [Fact]
        public void Rejects_a_blank_name()
        {
            // Et tomt navn giver ingen mening, så det skal kaste en fejl i stedet for at gemme.
            Assert.Throws<ValidationException>(() => CategoriesController.Create(new CategoryCreateRequest(" ")));
        }
    }

    public class UpdateTests : ApiTest
    {
        [Fact]
        public void Renames_the_category()
        {
            Db.Insert(new Category { Id = "1", Name = "Old name" });

            CategoriesController.Update(new CategoryUpdateRequest("1", "New name"));

            // Hent kategorien direkte fra databasen, og tjek at navnet faktisk blev ændret.
            Assert.Equal("New name", Db.Categories().First(c => c.Id == "1").Name);
        }

        [Fact]
        public void Throws_when_the_category_does_not_exist()
        {
            // "nope" findes ikke som ID, så det skal fejle i stedet for at gøre ingenting.
            Assert.Throws<KeyNotFoundException>(() =>
                CategoriesController.Update(new CategoryUpdateRequest("nope", "New name")));
        }
    }

    public class DeleteTests : ApiTest
    {
        [Fact]
        public void Removes_a_category_with_no_products()
        {
            Db.Insert(new Category { Id = "1", Name = "Drugs" });

            CategoriesController.Delete("1");

            // Databasen skal være tom for kategorier bagefter.
            Assert.Empty(Db.Categories().ToList());
        }

        [Fact]
        public void Refuses_while_a_product_still_uses_it()
        {
            Db.Insert(new Category { Id = "1", Name = "Drugs" });
            // Opret et produkt, der peger på denne kategori (CategoryId = "1").
            Db.Insert(new Product
            {
                Id = "p1", Name = "X", Description = "Y", PriceDkk = 1, StockCount = 1,
                CategoryId = "1", VendorId = "v1", CreatedAtUtc = DateTime.UtcNow
            });

            // Kategorien er stadig i brug, så sletning skal nægtes.
            Assert.Throws<InvalidOperationException>(() => CategoriesController.Delete("1"));
        }
    }
}