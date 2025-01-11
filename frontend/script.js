// Dinamik sahifa yuklash
const content = document.getElementById('content');

// Home sahifasini yaratish
function loadHomePage() {
  content.innerHTML = `
    <h2>Welcome to NewEra E-Commerce!</h2>
    <p>Explore our products and enjoy shopping!</p>
  `;
}

// Savat sahifasini yaratish
function loadCartPage() {
  content.innerHTML = `
    <h2>Your Cart</h2>
    <p>No items in the cart.</p>
  `;
}

// Sahifani o'zgartirish
document.getElementById('home-link').addEventListener('click', (e) => {
  e.preventDefault();
  loadHomePage();
});

document.getElementById('cart-link').addEventListener('click', (e) => {
  e.preventDefault();
  loadCartPage();
});

function loadProducts() {
    content.innerHTML = '<h2>Products</h2><p>Loading...</p>';
  
    fetch('http://localhost:5000/api/products')
      .then(response => response.json())
      .then(products => {
        content.innerHTML = `
          <h2>Products</h2>
          <ul>
            ${products
              .map(product => `<li>${product.name} - $${product.price}</li>`)
              .join('')}
          </ul>
        `;
      })
      .catch(error => {
        content.innerHTML = `<p>Error loading products: ${error.message}</p>`;
      });
  }
  
  // Home sahifasini yangilashda mahsulotlarni ko'rsatish
  function loadHomePage() {
    content.innerHTML = `<h2>Welcome to NewEra E-Commerce!</h2>`;
    loadProducts();
  }
  

// Boshlang'ich yuklash
loadHomePage();
