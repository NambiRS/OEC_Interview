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
export const getUsersByProcedure = async (procedureId, planId) => {
    try {
        const url = `${api_url}/Procedures/Users?procedureId=${procedureId}&planId=${planId}`;
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
export const assignUserToProcedure = async (procedureId, userId, planId) => {
    try {
        const url = `${api_url}/Procedures/Users`;
        const body = {
            procedureId: parseInt(procedureId),
            userId: parseInt(userId),
            planId: parseInt(planId),
        };
        
        console.log("assignUserToProcedure - Request:", { url, body });
        
        const response = await fetch(url, {
            method: "POST",
            headers: {
                Accept: "application/json",
                "Content-Type": "application/json",
            },
            body: JSON.stringify(body),
        });

        console.log("assignUserToProcedure - Response status:", response.status);
        
        if (!response.ok) {
            const errorText = await response.text();
            console.error("assignUserToProcedure - Error response:", errorText);
            throw new Error(`Failed to assign user to procedure. Status: ${response.status}, Error: ${errorText}`);
        }

        const result = await response.json();
        console.log("assignUserToProcedure - Success:", result);
        return result;
    } catch (error) {
        console.error("assignUserToProcedure error:", error);
        return null;
    }
};

// Remove a single user from a procedure by ids
export const removeUserFromProcedure = async (procedureId, userId, planId) => {
    try {
        const url = `${api_url}/Procedures/Users/ByIds`;
        const body = {
            procedureId,
            userId,
            planId,
        };
        const response = await fetch(url, {
            method: "DELETE",
            headers: {
                Accept: "application/json",
                "Content-Type": "application/json",
            },
            body: JSON.stringify(body),
        });

        if (!response.ok) throw new Error("Failed to remove user from procedure");

        return await response.json();
    } catch (error) {
        console.error("removeUserFromProcedure error:", error);
        return null;
    }
};

// Remove all users from a procedure by procedureId and planId
export const removeAllUsersFromProcedure = async (procedureId, planId) => {
    try {
        const url = `${api_url}/Procedures/Users/ByProcedure?procedureId=${procedureId}&planId=${planId}`;
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

