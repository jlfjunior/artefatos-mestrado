const API_URL = "http://localhost:5001/api/Authentication/login";

export const login = async (username, password) => {
    const response = await fetch(API_URL, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username, password }),
    });

    var result = await response.json();

    if (!result.success) {
        throw new Error(result.message);
    }

    return result;
};
