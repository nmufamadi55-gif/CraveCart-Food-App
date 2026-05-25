let restaurants = window.craveCartRestaurants || [];
let currentDiscount = 0;
let currentPointsDiscount = 0;
let driverInterval = null;
let activeFilter = "All";

function money(value) {
  return `R${Number(value).toFixed(2)}`;
}

function categoryAccent(category) {
  if (category === "Premium") return "#8f4cc2";
  if (category === "Healthy") return "#1f9d69";
  return "#f05a28";
}

function fallbackImage(name, color) {
  return "/images/backgrounds/food-delivery-bg.jpg?v=real4";
}

function versionedImage(url) {
  if (!url || !url.startsWith("/images/")) return url;
  return url.includes("?") ? url : `${url}?v=real4`;
}

function showToast(msg, isErr) {
  const toast = $("#toast");
  toast
    .text(msg)
    .css("background", isErr ? "#d63f2f" : "#1f9d69")
    .fadeIn(160);
  setTimeout(() => toast.fadeOut(260), 3000);
}

function loadPoints() {
  $.get("/Home/GetLoyaltyPoints", function (data) {
    $("#pointsAvailable").text(`You have ${data.points} points. 10 points = R1.`);
  });
}

function restaurantCard(restaurant) {
  const accent = categoryAccent(restaurant.category);
  const image = versionedImage(restaurant.imageUrl) || fallbackImage(restaurant.name, accent);
  return `
    <article class="restaurant-card" onclick="openRestaurant('${restaurant.id}')">
      <div class="restaurant-image">
        <img src="${image}" alt="${restaurant.name}" onerror="this.src='${fallbackImage(restaurant.name, accent)}'">
      </div>
      <div class="restaurant-card-body">
        <span class="popular-badge">${restaurant.category}</span>
        <h3 class="restaurant-name">${restaurant.name}</h3>
        <div class="restaurant-meta">
          <span>${restaurant.rating} rating</span>
          <span>${restaurant.reviews}+ reviews</span>
          <span>${restaurant.deliveryTime}</span>
        </div>
      </div>
    </article>`;
}

function displayRestaurants() {
  const groups = ["Fast Food", "Premium", "Healthy"];
  let html = "";

  groups.forEach((category) => {
    const list = restaurants.filter((restaurant) =>
      restaurant.category === category && (activeFilter === "All" || activeFilter === category)
    );

    if (!list.length) return;

    html += `
      <section class="category">
        <div class="category-title">
          <h3>${category}</h3>
          <span>${list.length} options</span>
        </div>
        <div class="restaurant-grid">
          ${list.map(restaurantCard).join("")}
        </div>
      </section>`;
  });

  $("#restaurantsContainer").html(html || `<div class="empty-state"><strong>No restaurants found</strong><span>Try another category.</span></div>`);
}

function openRestaurant(id) {
  const restaurant = restaurants.find((item) => item.id === id);
  if (!restaurant) return;

  const accent = categoryAccent(restaurant.category);
  const headerImage = versionedImage(restaurant.imageUrl) || fallbackImage(restaurant.name, accent);
  $("#modalTitle").html(`
    <div class="modal-title-content">
      <img src="${headerImage}" alt="${restaurant.name}" onerror="this.src='${fallbackImage(restaurant.name, accent)}'">
      <div>
        <h2>${restaurant.name}</h2>
        <p>${restaurant.rating} rating &bull; ${restaurant.deliveryTime}</p>
      </div>
    </div>`);

  const html = restaurant.menu.map((item) => {
    const itemImage = versionedImage(item.imageUrl) || fallbackImage(item.name, accent);
    const serializedItem = JSON.stringify(item).replace(/"/g, "&quot;");
    return `
      <article class="menu-item" onclick="showCustom('${restaurant.id}','${restaurant.name}',${serializedItem})">
        <img src="${itemImage}" alt="${item.name}" onerror="this.src='${fallbackImage(item.name, accent)}'">
        <div class="menu-item-info">
          <h4>${item.name} ${item.isPopular ? '<span class="popular-badge">Popular</span>' : ""}</h4>
          <p>${item.description || ""}</p>
        </div>
        <div class="menu-actions">
          <span class="menu-price">R${item.price}</span>
          <button class="add-btn" type="button">Add</button>
        </div>
      </article>`;
  }).join("");

  $("#menuList").html(html);
  $("#restaurantModal").css("display", "flex");
}

function showCustom(restId, restName, item) {
  const restaurant = restaurants.find((entry) => entry.id === restId);
  const accent = categoryAccent(restaurant ? restaurant.category : "Fast Food");
  const itemImage = versionedImage(item.imageUrl) || fallbackImage(item.name, accent);

  let html = `
    <div class="custom-hero">
      <img src="${itemImage}" alt="${item.name}" onerror="this.src='${fallbackImage(item.name, accent)}'">
      <h3>${item.name} - R${item.price}</h3>
      <p>${item.description || ""}</p>
    </div>
    <h4>Customizations</h4>`;

  if (item.customizations && item.customizations.length) {
    item.customizations.forEach((customization) => {
      const icon = customization.icon && customization.icon.startsWith("/")
        ? `<img src="${versionedImage(customization.icon)}" alt="" onerror="this.style.display='none'">`
        : `<span>${customization.icon || "+"}</span>`;
      html += `
        <label class="custom-option">
          <input type="checkbox" value="${customization.name}" data-price="${customization.price}">
          ${icon}
          <span>${customization.name} (+R${customization.price})</span>
        </label>`;
    });
  } else {
    html += `<p class="cart-meta">No customizations available for this item.</p>`;
  }

  html += `
    <div class="custom-actions">
      <button class="place-order-btn" type="button" onclick="addToCart('${restId}','${restName}',${item.id},'${item.name}',${item.price},'${item.type}')">Add to Cart</button>
      <button class="cancel-btn" type="button" onclick="closeCustom()">Cancel</button>
    </div>`;

  $("#customContent").html(html);
  $("#customModal").css("display", "flex");
}

function addToCart(restId, restName, itemId, itemName) {
  const extras = [];
  $("#customModal input:checked").each(function () {
    extras.push($(this).val());
  });

  $.post("/Home/AddToCart", { restaurantId: restId, itemId: itemId, extras: extras }, function (response) {
    if (response.success) {
      $("#cartCount").text(response.cartCount);
      showToast(`${itemName} added to cart`, false);
      closeCustom();
      closeModal();
    } else {
      showToast(response.message, true);
    }
  }).fail(() => showToast("Error adding to cart", true));
}

function loadCart() {
  $.get("/Home/GetCart", function (data) {
    if (!data.items || !data.items.length) {
      $("#cartItems").html(`<div class="empty-state"><strong>Your cart is empty</strong><span>Add a meal to start checkout.</span></div>`);
      $("#cartSummary").hide();
      return;
    }

    const html = data.items.map((item) => {
      const image = item.itemIcon && item.itemIcon.startsWith("/") ? versionedImage(item.itemIcon) : fallbackImage(item.itemName, "#f05a28");
      return `
        <div class="cart-item">
          <div class="cart-item-main">
            <img src="${image}" alt="${item.itemName}" onerror="this.src='${fallbackImage(item.itemName, "#f05a28")}'">
            <span class="cart-item-name">${item.itemName}</span>
            <button class="remove-btn" type="button" onclick="removeFromCart(${item.index})" aria-label="Remove ${item.itemName}">x</button>
          </div>
          <div class="cart-meta">${item.restaurantName}</div>
          ${item.extras && item.extras.length ? `<div class="cart-extras">+ ${item.extras.join(", ")}</div>` : ""}
          <div class="cart-price">${money(item.totalPrice)}</div>
        </div>`;
    }).join("");
    $("#cartItems").html(html);

    let total = data.total - currentDiscount - currentPointsDiscount;
    if (total < 0) total = 0;

    $("#cartSummary").html(`
      <div class="summary">
        <div class="summary-row"><span>Subtotal</span><span>${money(data.subtotal)}</span></div>
        <div class="summary-row"><span>Delivery</span><span>${money(data.deliveryFee)}</span></div>
        <div class="promo-group">
          <input type="text" id="promoCode" placeholder="Promo code">
          <button type="button" onclick="applyPromo()">Apply</button>
        </div>
        <div class="points-group">
          <input type="number" id="pointsInput" placeholder="Loyalty points">
          <button type="button" onclick="applyPoints()">Use</button>
        </div>
        <div id="pointsAvailable" class="cart-meta"></div>
        <div class="summary-row">
          <span>Payment</span>
          <select id="paymentMethod" class="payment-select">
            <option>Credit Card</option>
            <option>EFT</option>
            <option>PayFast</option>
          </select>
        </div>
        <div class="summary-row total"><span>Total</span><span>${money(total)}</span></div>
        <button class="place-order-btn" type="button" onclick="placeOrder()">Place Order</button>
      </div>
    `).show();
    loadPoints();
  });
}

function saveCart() {
  if ($("#cartCount").text() == 0) {
    showToast("Cart is empty. Nothing to save.", true);
    return;
  }

  $.post("/Home/SaveCart", function (response) {
    showToast(response.message || "Cart saved");
  });
}

function loadCartSave() {
  $.post("/Home/LoadCart", function (response) {
    showToast(response.message, !response.success);
    if (response.success) setTimeout(() => location.reload(), 800);
  });
}

const driverStages = [
  { status: "Searching for driver", eta: 8, progress: 20 },
  { status: "Driver assigned", eta: 5, progress: 40 },
  { status: "Driver en route to restaurant", eta: 3, progress: 60 },
  { status: "Driver at restaurant", eta: 2, progress: 75 },
  { status: "Driver on the way", eta: 5, progress: 85 },
  { status: "Nearby", eta: 5, progress: 95 },
  { status: "Arriving now", eta: 2, progress: 98 },
  { status: "Delivered. Enjoy your meal", eta: 0, progress: 100 }
];
let currentStage = 0;

function startDriverTracking() {
  $("#driverFloatingPanel").show();
  if (driverInterval) clearInterval(driverInterval);
  currentStage = 0;
  updateDriverDisplay();

  driverInterval = setInterval(function () {
    if (currentStage < driverStages.length - 1) {
      currentStage++;
      updateDriverDisplay();
      if (currentStage === driverStages.length - 1) {
        clearInterval(driverInterval);
        addNotification("Order delivered successfully");
      }
    } else {
      clearInterval(driverInterval);
    }
  }, 4000);
}

function updateDriverDisplay() {
  const stage = driverStages[currentStage];
  $("#driverName").text("Michael");
  $("#driverStatus").html(`<span class="status-dot"></span>${stage.status}`);
  $("#driverEta").text(stage.eta > 0 ? `${stage.eta} minutes` : "Arrived");
  $("#etaProgress").css("width", `${stage.progress}%`);

  let historyHtml = "<strong>Trip Timeline</strong>";
  for (let index = 0; index <= currentStage; index++) {
    const marker = index === currentStage ? "Now" : "Done";
    historyHtml += `<div>${marker}: ${driverStages[index].status}</div>`;
  }
  $("#driverHistory").html(historyHtml);
}

function closeDriverPanel() {
  $("#driverFloatingPanel").hide();
  if (driverInterval) clearInterval(driverInterval);
}

function addNotification(message) {
  const time = new Date().toLocaleTimeString();
  $("#orderNotifications").prepend(`<div class="notification-item"><span class="notification-time">[${time}]</span> ${message}</div>`);
  if ($("#orderNotifications .notification-item").length > 15) {
    $("#orderNotifications .notification-item:last").remove();
  }
}

function applyPromo() {
  const code = $("#promoCode").val();
  if (!code) {
    showToast("Please enter a promo code", true);
    return;
  }

  $.post("/Home/ApplyPromo", { promoCode: code }, function (response) {
    if (response.success) {
      currentDiscount = response.discount;
      showToast(response.message);
      loadCart();
    } else {
      showToast(response.message, true);
    }
  });
}

function applyPoints() {
  const points = $("#pointsInput").val();
  if (!points || points <= 0) {
    showToast("Please enter valid points", true);
    return;
  }

  $.post("/Home/ApplyPoints", { points: points }, function (response) {
    if (response.success) {
      currentPointsDiscount = response.discount;
      showToast(`${points} points applied. Saved ${money(response.discount)}`);
      loadCart();
    }
  });
}

function removeFromCart(index) {
  $.post("/Home/RemoveFromCart", { index: index }, function () {
    loadCart();
    updateCount();
    showToast("Item removed", false);
  });
}

function clearCart() {
  if (confirm("Clear entire cart?")) {
    $.post("/Home/ClearCart", function () {
      loadCart();
      updateCount();
      showToast("Cart cleared", false);
    });
  }
}

function updateCount() {
  $.get("/Home/GetCart", function (data) {
    $("#cartCount").text(data.itemCount || 0);
  });
}

function placeOrder() {
  const paymentMethod = $("#paymentMethod").val();
  const total = parseFloat($(".total span:last").text().replace("R", ""));

  if (!total || total <= 0) {
    showToast("Cart is empty. Add items first.", true);
    return;
  }

  startDriverTracking();
  $("#notificationPanel").show();
  $("#orderNotifications").html('<div class="notification-item">Processing your order...</div>');
  addNotification("Connecting to restaurant...");
  setTimeout(() => addNotification("Order received by restaurant"), 500);
  setTimeout(() => addNotification("Chef is preparing your meal"), 1000);
  setTimeout(() => addNotification("Meal is ready. Packing in progress"), 2000);
  setTimeout(() => addNotification("Driver assigned to your order"), 2500);
  setTimeout(() => addNotification("Driver is on the way"), 3000);

  $.post("/Home/PlaceOrder", { paymentMethod: paymentMethod, total: total }, function (response) {
    if (response.success) {
      addNotification("Order completed successfully");
      addNotification(`Earned ${response.pointsEarned} loyalty points`);
      addNotification(response.loggerSummary);

      setTimeout(() => {
        alert(`Order placed successfully!\n\n${response.loggerSummary}\n\nEarned ${response.pointsEarned} points.\nTotal points: ${response.totalPoints}`);
        closeCart();
        updateCount();
        currentDiscount = 0;
        currentPointsDiscount = 0;
        setTimeout(() => {
          $("#notificationPanel").hide();
          $("#orderNotifications").empty();
        }, 8000);
      }, 1000);
    } else {
      addNotification(`Order failed: ${response.message}`);
      showToast("Order failed", true);
    }
  }).fail(() => {
    addNotification("Network error. Please try again.");
    showToast("Error placing order", true);
  });
}

function toggleCart() {
  $("#cartSidebar").toggleClass("open");
  $("#overlay").toggleClass("active");
  if ($("#cartSidebar").hasClass("open")) loadCart();
}

function closeCart() {
  $("#cartSidebar").removeClass("open");
  $("#overlay").removeClass("active");
}

function closeModal() {
  $("#restaurantModal").hide();
}

function closeCustom() {
  $("#customModal").hide();
}

$(document).ready(function () {
  displayRestaurants();
  updateCount();

  $(".category-tab").on("click", function () {
    activeFilter = $(this).data("filter");
    $(".category-tab").removeClass("active");
    $(this).addClass("active");
    displayRestaurants();
  });

  $(document).on("click", function (event) {
    if ($(event.target).hasClass("modal")) {
      closeModal();
      closeCustom();
    }
  });
});
