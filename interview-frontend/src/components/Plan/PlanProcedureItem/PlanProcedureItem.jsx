import React, { useEffect, useState, useCallback } from "react";
import ReactSelect from "react-select";
import {
    getUsersByProcedure,
    assignUserToProcedure,
    removeUserFromProcedure,
    removeAllUsersFromProcedure,
} from "../../../api/api";

const userToOption = user => ({
    value: user.userId ?? user.value,
    label: user.name ?? user.label,
    procedureUserId: user.procedureUserId,
    ...user,
});

const PlanProcedureItem = ({ procedure, users }) => {
    const [selectedUsers, setSelectedUsers] = useState([]);
    const [userOptions, setUserOptions] = useState([]);

    // Refresh user options and assigned users
    const refreshUserLists = useCallback(async () => {
        const allUsers = Array.isArray(users) ? users : [];

        // Fetch assigned users for this procedure
        const assigned = await getUsersByProcedure(procedure.procedureId);
        const assignedOptions = assigned.map(userToOption);
        setSelectedUsers(assignedOptions);

        // Filter out assigned users from all users for dropdown
        const filteredUsers = allUsers
            .filter(u => !assigned.some(a => a.userId === (u.userId ?? u.value)))
            .map(userToOption);

        // Ensure assigned users are always visible in dropdown
        setUserOptions([
            ...filteredUsers,
            ...assignedOptions.filter(
                su => !filteredUsers.some(au => au.value === su.value)
            ),
        ]);
    }, [procedure.procedureId, users]);

    useEffect(() => {
        refreshUserLists();
    }, [refreshUserLists]);

    const handleAssignUserToProcedure = async (selected) => {
        const prevIds = selectedUsers.map(u => u.value);
        const newUsers = selected.filter(u => !prevIds.includes(u.value));
        const removedUsers = selectedUsers.filter(
            u => !selected.some(su => su.value === u.value)
        );

        // Add new users
        for (const user of newUsers) {
            await assignUserToProcedure(procedure.procedureId, user.value);
        }

        // Remove users
        if (removedUsers.length > 1) {
            await removeAllUsersFromProcedure(procedure.procedureId);
        } else if (removedUsers.length === 1) {
            const user = removedUsers[0];
            if (user.procedureUserId) {
                await removeUserFromProcedure(user.procedureUserId);
            }
        }

        await refreshUserLists();
    };

    return (
        <div className="py-2">
            <div>{procedure.procedureTitle}</div>
            <ReactSelect
                className="mt-2"
                placeholder="Select User to Assign"
                isMulti
                options={userOptions}
                value={selectedUsers}
                onChange={handleAssignUserToProcedure}
            />
        </div>
    );
};

export default PlanProcedureItem;
