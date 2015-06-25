administration.controller('UsersController', ['$scope', '$http', '$resource', UsersController]);

function UsersController($scope, $http, $resource) {

    CreateUserAdminServices($resource);

    $scope.Locations = Locations.query({});
    $scope.admins = Admins.getAll({});
    $scope.adminId = 0;

    $scope.SetAdminId = function (id) {
        $scope.adminId = id;
    }

    $scope.AddAdmin = function () {
        SetAdminErrorMessage('');

        //Validate Location
        if ($scope.adminUser.Location.name === undefined) {
            SetAdminErrorMessage('Invalid Location.');
            return;
        }
        else {
            $scope.adminUser.Location = $scope.adminUser.Location.name;
        }

        //Validate First & Last Name
        if ($scope.adminUser.FirstName === "" || $scope.adminUser.LastName === "") {
            SetAdminErrorMessage('Invalid First/Last Name.');
            return;
        }

        //Validate Email
        if (!ValidateEmail($scope.adminUser.Email)) {
            SetAdminErrorMessage('Invalid Email Detected.');
            return;
        }

        $.ajax({
            type: "POST",
            data: JSON.stringify($scope.adminUser),
            url: "api/admin",
            contentType: "application/json",
            success: function () {
                alert('User Added Successfully!');
                location.reload();
            },
            error: function (error) {
                if (error.status == 409) {
                    SetAdminErrorMessage('Administrator Already Exists!');
                }
                else {
                    alert('Unable to Add Admin. Please Contact ITSD. ' + error.message);
                }
            }
        });

        SetAdminErrorMessage('');
    }

    $scope.EditAdmin = function (id) {
        
    }

    $scope.DeleteAdmin = function () {
        Admins.removeAdminById({
            adminId: $scope.adminId
        },
        function (success) {
            location.reload();
        },
        function (error) {
            alert('Unable to Delete Admin. Please Try Again or Contact ITSD. ' + error.message); //TODO:Log Error
        })
    };

    $('#activeDirecotryModal').on('show.bs.modal', function () {
        $scope.GetAdNames();
    });

    $scope.GetAdNames = function () {
        if ($scope.emailVal === undefined || $scope.emailVal.trim() === "") {
            $scope.adAdminUsers = AdUsers.queryAll({
                allUsers: true
            })
        }
        else {
            $scope.adAdminUsers = AdUsers.querySurname({
                allUsers: $scope.emailVal
            });
        }
    }

    $scope.AddAdEmail = function (user) {
        $scope.adminUser.PID = user.pid;
        $scope.adminUser.FirstName = user.firstName;
        $scope.adminUser.LastName = user.lastName;
        $scope.adminUser.Email = user.email;

        $('#activeDirecotryModal').modal('hide');
    }

    $scope.adminUser = {
        PID: '',
        FirstName: '',
        LastName: '',
        Location: '',
        Email: '',
        PermissionLevel: 1
    }

    $scope.adAdminUser = {
        pID: '',
        firstName: '',
        lastName: '',
        email: ''
    };
}

function CreateUserAdminServices($resource) {
    Admins = $resource('/api/admin/:adminId', { adminId: 'adminId' },
    {
        getAll: { method: 'GET', isArray: true },
        getAdminById: { method: 'GET', isArray: true },
        removeAdminById: { method: 'DELETE' }
    });

    Admins.prototype = {
        permissionLevelText: function () { return GetPermissionText(this.permissionLevel); }
    };

    Locations = $resource('/api/locations', {}, {
        query: { method: 'GET', isArray: true }
    });

    AdUsers = $resource('/api/users/:allUsers', { allUsers: 'allUsers' },
    {
        queryAll: { method: 'GET', isArray: true },
        querySurname: { method: 'GET', isArray: true }
    });
}

function SetAdminErrorMessage(message) {
    if (message === "") {
        $('#adminErrorMessage').hide();
    }
    else {
        $('#adminErrorMessage').text(message);
        $('#adminErrorMessage').show();
    }
}

function GetPermissionText(permission) {
    switch (permission) {
        case 1: return "Admin";
        case 2: return "Super Admin";
    }
}

$(document).ready(function () {
    $("#pidTextBox").keydown(function (e) {
            e.preventDefault();
    });
});