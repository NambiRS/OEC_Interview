import React, { useState, useEffect } from "react";
import ReactSelect from "react-select";
import {
    getUsersByProcedure,
    assignUserToProcedure,
    removeUserFromProcedure,
    removeAllUsersFromProcedure,
} from "../../../api/api";

const PlanProcedureItem = ({ procedure, planId, users }) => {
    const [selectedUsers, setSelectedUsers] = useState([]);
    const [assignedUsers, setAssignedUsers] = useState([]);

    // Debug: Log the users prop to see its format
    console.log("Users prop (options):", users);

    // Load assigned users when component mounts or procedure changes
    useEffect(() => {
        loadAssignedUsers();
    }, [procedure.procedureId, planId]);

    const loadAssignedUsers = async () => {
        try {
            const users = await getUsersByProcedure(procedure.procedureId, planId);
            console.log("Raw assigned users from API:", users);
            if (users && Array.isArray(users)) {
                setAssignedUsers(users);
                // Map assigned users to selected format for ReactSelect
                // Ensure the format matches exactly with the options format
                const mappedUsers = users.map(user => {
                    console.log("Mapping user:", user); // Debug individual user object
                    return {
                        label: user.name || user.userName || user.userLabel || `User ${user.userId}`, // Try different possible name properties
                        value: parseInt(user.userId) // Convert to number to match options format
                    };
                });
                console.log("Mapped assigned users for ReactSelect:", mappedUsers);
                setSelectedUsers(mappedUsers);
            } else {
                // If no users or invalid response, clear the states
                setAssignedUsers([]);
                setSelectedUsers([]);
            }
        } catch (error) {
            console.error("Error loading assigned users:", error);
            setAssignedUsers([]);
            setSelectedUsers([]);
        }
    };

    const handleAssignUserToProcedure = async (selectedOptions) => {
        // Handle null/undefined selectedOptions (when all users are cleared)
        const safeSelectedOptions = selectedOptions || [];
        
        const currentUserIds = selectedUsers.map(u => u.value);
        const newUserIds = safeSelectedOptions.map(u => u.value);
        
        // Find newly added users
        const addedUsers = safeSelectedOptions.filter(option => !currentUserIds.includes(option.value));
        
        // Find removed users
        const removedUsers = selectedUsers.filter(user => !newUserIds.includes(user.value));

        try {
            // Add new users
            for (const user of addedUsers) {
                console.log("Adding user:", { procedureId: procedure.procedureId, userId: user.value, planId });
                
                // Validate required parameters
                if (!procedure.procedureId || !user.value || !planId) {
                    console.error("Missing required parameters:", { 
                        procedureId: procedure.procedureId, 
                        userId: user.value, 
                        planId 
                    });
                    continue;
                }
                
                await assignUserToProcedure(procedure.procedureId, user.value, planId);
            }

            // Remove users - optimize by using removeAllUsersFromProcedure if removing all users
            if (removedUsers.length > 0) {
                if (newUserIds.length === 0 && currentUserIds.length > 0) {
                    // If no users are selected and there were previously assigned users, remove all
                    await removeAllUsersFromProcedure(procedure.procedureId, planId);
                } else {
                    // Remove users one by one
                    for (const user of removedUsers) {
                        await removeUserFromProcedure(procedure.procedureId, user.value, planId);
                    }
                }
            }

            // Reload assigned users to get updated data
            await loadAssignedUsers();
        } catch (error) {
            console.error("Error updating user assignments:", error);
            // Revert to previous state on error
            setSelectedUsers(selectedUsers);
        }
    };

    return (
        <div className="py-2">
            <div>
                {procedure.procedureTitle}
            </div>

            <ReactSelect
                className="mt-2"
                placeholder="Select User to Assign"
                isMulti={true}
                options={users}
                value={selectedUsers}
                onChange={handleAssignUserToProcedure}
                isSearchable={true}
                closeMenuOnSelect={false}
            />
        </div>
    );
};

export default PlanProcedureItem;