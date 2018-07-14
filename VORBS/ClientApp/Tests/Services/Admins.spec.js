describe('Admins Service', function () {

    var AdminsService;
    var $httpBackend;

    beforeEach(angular.mock.module('vorbs.services'));

    beforeEach(inject(function (_AdminsService_, _$httpBackend_) {
        $httpBackend = _$httpBackend_;
        AdminsService = _AdminsService_;
    }));

    afterEach(function () {
        $httpBackend.verifyNoOutstandingExpectation();
    });

    it('should be defined', function () {
        expect(AdminsService).toBeDefined();
    });

    describe('update method', function () {

        it('should exist', function () {
            expect(AdminsService.update).not.toEqual(null);
            expect(AdminsService.update).toBeDefined();
        })

        it('should update an admin\'s details', function () {

            var admin = adminMockData.getAdminUser();

            var newPermissionLevel = 1;

            admin.permissionLevel = newPermissionLevel;

            $httpBackend.expectPUT('/api/admin/' + admin.id)
                .respond(200);

            AdminsService.update({
                id: admin.id
            }, admin);

            expect($httpBackend.flush).not.toThrow();
        });
    });

});