const api_url = "http://localhost:10010";

export const startPlan = async () => {
    const url = `${api_url}/Plan`;
    const response = await fetch(url, {
        method: "POST",
        headers: {
            Accept: "application/json",
            "Content-Type": "application/json",
        },
        body: JSON.stringify({}),
    });

    if (!response.ok) throw new Error("Failed to create plan");

    return await response.json();
};

export const addProcedureToPlan = async (planId, procedureId) => {
    const url = `${api_url}/Plan/AddProcedureToPlan`;
    var command = { planId: planId, procedureId: procedureId };
    const response = await fetch(url, {
        method: "POST",
        headers: {
            Accept: "application/json",
            "Content-Type": "application/json",
        },
        body: JSON.stringify(command),
    });

    if (!response.ok) throw new Error("Failed to create plan");

    return true;
};

export const getProcedures = async () => {
    const url = `${api_url}/Procedures`;
    const response = await fetch(url, {
        method: "GET",
    });

    if (!response.ok) throw new Error("Failed to get procedures");

    return await response.json();
};

export const getPlanProcedures = async (planId) => {
    const url = `${api_url}/PlanProcedure?$filter=planId eq ${planId}&$expand=procedure`;
    const response = await fetch(url, {
        method: "GET",
    });

    if (!response.ok) throw new Error("Failed to get plan procedures");

    return await response.json();
};

export const getUsers = async () => {
    const url = `${api_url}/Users`;
    const response = await fetch(url, {
        method: "GET",
    });

    if (!response.ok) throw new Error("Failed to get users");

    return await response.json();
};

// Get users assigned to a specific procedure
export const getUsersByProcedure = async (procedureId) => {
    const url = `${api_url}/Procedures/Users?procedureId=${procedureId}`;
    const response = await fetch(url, {
        method: "GET",
    });

    if (!response.ok) throw new Error("Failed to get users for procedure");

    return await response.json();
};

// Assign a single user to a procedure
export const assignUserToProcedure = async (procedureId, userId) => {
    const url = `${api_url}/Procedures/Users`;
    const body = {
        procedureId,
        userId,
    };
    const response = await fetch(url, {
        method: "POST",
        headers: {
            Accept: "application/json",
            "Content-Type": "application/json",
        },
        body: JSON.stringify(body),
    });

    if (!response.ok) throw new Error("Failed to assign user to procedure");

    return await response.json();
};

// Remove a single user from a procedure by procedureUserId
export const removeUserFromProcedure = async (procedureUserId) => {
    const url = `${api_url}/Procedures/User?procedureUserId=${procedureUserId}`;
    const response = await fetch(url, {
        method: "DELETE",
        headers: {
            Accept: "application/json",
            "Content-Type": "application/json",
        },
    });

    if (!response.ok) throw new Error("Failed to remove user from procedure");

    return await response.json();
};

// Remove all users from a procedure by procedureId
export const removeAllUsersFromProcedure = async (procedureId) => {
    const url = `${api_url}/Procedures/Users/ByProcedure?procedureId=${procedureId}`;
    const response = await fetch(url, {
        method: "DELETE",
        headers: {
            Accept: "application/json",
            "Content-Type": "application/json",
        },
    });

    if (!response.ok) throw new Error("Failed to remove all users from procedure");

    return await response.json();
};

