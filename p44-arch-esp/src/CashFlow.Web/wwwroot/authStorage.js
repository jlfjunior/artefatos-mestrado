window.cashFlowAuth = {
    get: () => sessionStorage.getItem("cashflow.auth"),
    set: (value) => sessionStorage.setItem("cashflow.auth", value),
    clear: () => sessionStorage.removeItem("cashflow.auth")
};
