const api_url = "http://localhost:10010";

export const startPlan = async () => {
    try {
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
    } catch (error) {
        console.error("startPlan error:", error);
        return null;
    }
};

export const addProcedureToPlan = async (planId, procedureId) => {
    try {
        const url = `${api_url}/Plan/AddProcedureToPlan`;
        const command = { planId, procedureId };
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
    } catch (error) {
        console.error("addProcedureToPlan error:", error);
        return false;
    }
};

export const getProcedures = async () => {
    try {
        const url = `${api_url}/Procedures`;
        const response = await fetch(url, {
            method: "GET",
        });

        if (!response.ok) throw new Error("Failed to get procedures");

        return await response.json();
    } catch (error) {
        console.error("getProcedures error:", error);
        return null;
    }
};

export const getPlanProcedures = async (planId) => {
    try {
        const url = `${api_url}/PlanProcedure?$filter=planId eq ${planId}&$expand=procedure`;
        const response = await fetch(url, {
            method: "GET",
        });

        if (!response.ok) throw new Error("Failed to get plan procedures");

        return await response.json();
    } catch (error) {
        console.error("getPlanProcedures error:", error);
        return null;
    }
};

export const getUsers = async () => {
    try {
        const url = `${api_url}/Users`;
        const response = await fetch(url, {
            method: "GET",
        });

        if (!response.ok) throw new Error("Failed to get users");

        return await response.json();
    } catch (error) {
        console.error("getUsers error:", error);
        return null;
    }
};

// Get users assigned to a specific procedure
export const getUsersByProcedure = async (procedureId) => {
    try {
        const url = `${api_url}/Procedures/Users?procedureId=${procedureId}`;
        const response = await fetch(url, {
            method: "GET",
        });

        if (!response.ok) throw new Error("Failed to get users for procedure");

        return await response.json();
    } catch (error) {
        console.error("getUsersByProcedure error:", error);
        return null;
    }
};

// Assign a single user to a procedure
export const assignUserToProcedure = async (procedureId, userId) => {
    try {
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
    } catch (error) {
        console.error("assignUserToProcedure error:", error);
        return null;
    }
};

// Remove a single user from a procedure by procedureUserId
export const removeUserFromProcedure = async (procedureUserId) => {
    try {
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
    } catch (error) {
        console.error("removeUserFromProcedure error:", error);
        return null;
    }
};

// Remove all users from a procedure by procedureId
export const removeAllUsersFromProcedure = async (procedureId) => {
    try {
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
    } catch (error) {
        console.error("removeAllUsersFromProcedure error:", error);
        return null;
    }
};

