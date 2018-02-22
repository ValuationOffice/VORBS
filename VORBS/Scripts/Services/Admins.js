(function () {

    angular.module('vorbs.services')
        .factory('AdminsService', AdminsService);

    AdminsService.$inject = ['$resource'];

    function AdminsService($resource) {
        return $resource('/api/admin/:id', { id: '@id' }, {
            update: {
                method: 'PUT'
            }
        });
    }

})();