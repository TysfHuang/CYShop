const productUri = "api/ProductApi";
const productSalesCountUri = "api/ProductSalesCountApi";
const cartList = [];
let shouldUpdatePaginedList = false;
let currentCategory = "ALL";
let currentSortOrder = "default";
let productTemplate;


async function GetFromApiController(targetUri) {
    try {
        const response = await fetch(targetUri);
        if (!response.ok) {
            throw new Error(`Http error: ${response.status}`);
        }
        return await response.json();
    } catch (error) {
        console.error(`Unable to get products: ${error}`);
    }
}

async function PostToApiController(targetUri, jsonObj) {
    try {
        const response = await fetch(targetUri, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(jsonObj)
        });
        if (!response.ok) {
            throw new Error(`Http error: ${response.status}`);
        }
        return await response.json();
    } catch (error) {
        console.error(`Unable to get products: ${error}`);
    }
}

function ShowLoadingSpinner(flag) {
    if (flag) {
        $("#loadingSpinner").show();
    } else {
        $("#loadingSpinner").hide();
    }
}

function GetAllProducts() {
    return GetFromApiController(productUri);
}

function GetProductsByCondiction(pageNum, searchString) {
    ShowLoadingSpinner(true);
    let targetUri = productUri + '/search/' + currentCategory;
    targetUri += '?sortOrder=' + currentSortOrder;
    targetUri += '&pageNum=' + pageNum;
    if (searchString != null) {
        targetUri += '&searchString=' + searchString;
    }
    return GetFromApiController(targetUri);
}

function GetProduct(id) {
    let targetUri = productUri + '/' + id.toString();
    return GetFromApiController(targetUri);
}

function GetCartItemObjectFormat(id, name, price, quantity) {
    return {
        ProductID: id,
        ProductName: name,
        Price: price,
        Quantity: quantity
    };
}

function AddToCart(productObj) {
    let targetUri = 'api/CartApi';
    const promise = PostToApiController(targetUri, productObj);
    promise
        .then((data) => {
            if (data != null && data.length > 0) {
                for (let i = 0; i < data.length; i++) {
                    cartList[i] = data[i];
                }
                UpdateCartSummary();
            }
        });
}

function CheckAndShowCheckoutButton() {
    if (cartList.length > 0) {
        $("#cartCheckout").attr("class", "btn btn-primary");
    }
}

function UpdateCartSummary() {
    let quantity = 0;
    let totalPrice = 0;
    for (let i = 0; i < cartList.length; i++) {
        quantity += cartList[i].quantity;
        totalPrice += cartList[i].price;
    }
    $("#cartSummary").text("購物車(" + quantity.toString() + ") 總價: $" + totalPrice.toString());
    CheckAndShowCheckoutButton();
}

function GetProductCard(id, title, price, imageUrl) {
    let product = productTemplate.clone();
    product.find("p.visually-hidden").text(id.toString());
    product.find("img").attr("src", imageUrl);
    product.find("h5.card-title").text(title);
    product.find("h6").next().text(price);
    product.find("button").click(function () {
        let productId = $(this).siblings("p.visually-hidden").text();
        let productName = $(this).parentsUntil("div.col").find("h5.card-title").text()
        let productPrice = parseInt($(this).parent().siblings("div.col-4").find("h6").text().replace("售價:", ""));
        AddToCart(GetCartItemObjectFormat(productId, productName, productPrice, 1));
    });
    return product;
}

function SetPaginedList(totalPage) {
    $("#paginedList").empty();
    for (let i = 1; i <= totalPage; i++) {
        let a = $('<a class="page-link" href="#">1</a>').text(i.toString());
        let button1 = $("<li></li>").click(function () {
            $(this).siblings().attr("class", "page-item");
            $(this).attr("class", "page-item active");
            let promises = GetProductsByCondiction(i, $("#searchString").val());
            SetProductListAndCloseLoadingSpinner(promises);
        });
        button1.append(a);
        $("#paginedList").append(button1);
    }
    ActivePaginedListFirstButton();
}

function SetProductListAsync(promises) {
    promises.then((data) => {
        if (data == null) {
            $("#productList").empty();
            $("#productList").append("<p>沒有搜尋到符合的產品</p>");
            SetPaginedList(1);
            return;
        }
        SetPageProductList(data["data"]);
        if (shouldUpdatePaginedList) {
            SetPaginedList(data["maxPageNum"]);
            shouldUpdatePaginedList = false;
        }
    });
}

function SetPageProductList(newProductList) {
    $("#productList").empty();
    if (newProductList.length == 0) {
        $("#productList").append("<p>沒有搜尋到符合的產品</p>");
        return;
    }
    let productList = $("#productList");
    for (let i = 0; i < newProductList.length; i++) {
        let imgPath = newProductList[i]["coverImagePath"] == 'need to update'
            ? '/example.jpg'
            : newProductList[i]["coverImagePath"];
        productList.append(
            GetProductCard(
                newProductList[i]["id"],
                newProductList[i]["name"],
                newProductList[i]["price"],
                imgPath));
    }
}

function InitialPage() {
    shouldUpdatePaginedList = true;
    productTemplate = $("#productList").children().first().clone();
    let promises = GetProductsByCondiction(1, null);
    SetProductListAndCloseLoadingSpinner(promises);
}

function GetCurrentCart() {
    let targetUri = 'api/CartApi';
    let promise = GetFromApiController(targetUri);
    promise.then((data) => {
        if (data != null && data.length > 0) {
            for (let i = 0; i < data.length; i++) {
                cartList[i] = data[i];
            }
            UpdateCartSummary();
        }
    });
}

function SetProductListAndCloseLoadingSpinner(promises) {
    SetProductListAsync(promises);
    ShowLoadingSpinner(false);
}

function SearchProductEvent() {
    let promises = GetProductsByCondiction(1, $("#searchString").val());
    shouldUpdatePaginedList = true;
    SetProductListAndCloseLoadingSpinner(promises);
}

function ProductCategoryEvent() {
    $("#sidebarProductCategory a").attr("class", "nav-link text-white");
    $(this).children("a").attr("class", "nav-link active");

    currentCategory = $(this).text().trim();
    shouldUpdatePaginedList = true;
    let promises = GetProductsByCondiction(1, null);
    SetProductListAndCloseLoadingSpinner(promises);
    $("#searchString").val("");
}

function ToggleSortOrderButtonState(clickButtonId) {
    $("#sortOrder").children("button").attr("class", "btn btn-outline-success");
    $(clickButtonId).attr("class", "btn btn-outline-success active");
}

function SetSortOrderPriceButtonToDefault() {
    $("#sortPrice").text("價格排序");
}

function ActivePaginedListFirstButton() {
    $("#paginedList").children().attr("class", "page-item");
    $("#paginedList li:first-child").attr("class", "page-item active");
}

function SortOrderDefaultEvent() {
    currentSortOrder = "all";
    ToggleSortOrderButtonState("#sortDefault");
    SetSortOrderPriceButtonToDefault();
    let promises = GetProductsByCondiction(1, $("#searchString").val());
    SetProductListAndCloseLoadingSpinner(promises);
    ActivePaginedListFirstButton();
}

function SortOrderPriceEvent() {
    if (currentSortOrder == "priceLowToHigh") {
        $("#sortPrice").text("價格高至低");
        currentSortOrder = "priceHighToLow";
    } else if (currentSortOrder == "priceHighToLow") {
        $("#sortPrice").text("價格低至高");
        currentSortOrder = "priceLowToHigh";
    } else {
        $("#sortPrice").text("價格低至高");
        currentSortOrder = "priceLowToHigh";
    }
    ToggleSortOrderButtonState("#sortPrice");
    let promises = GetProductsByCondiction(1, $("#searchString").val());
    SetProductListAndCloseLoadingSpinner(promises);
    ActivePaginedListFirstButton();
}

function SetHotSalesList() {
    let promises = GetFromApiController(productSalesCountUri);
    promises.then((data) => {
        if (data == null) {
            return;
        }
        for (let i = 0; i < data.length; i++) {
            let listitem = $("#sidebarHotSalesList div").first().clone();
            listitem.find("h5").text(data[i].name);
            listitem.find("div p").first().text("價格:" + data[i].price.toString());
            listitem.find("div p small").text("銷售數:" + data[i].salesCount.toString());
            listitem.find("div img").attr("src", data[i].coverImagePath);
            $("#sidebarHotSalesList").append(listitem);
        }
        $("#sidebarHotSalesList div").first().remove();
    });
}

$().ready(function () {
    InitialPage();
    SetHotSalesList();
    GetCurrentCart();
    $("#searchBtn").click(SearchProductEvent);
    $("#sidebarProductCategory").children().click(ProductCategoryEvent)
    $("#sortDefault").click(SortOrderDefaultEvent);
    $("#sortPrice").click(SortOrderPriceEvent);
});