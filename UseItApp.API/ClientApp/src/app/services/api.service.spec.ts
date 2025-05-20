import { TestBed } from '@angular/core/testing';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ApiService } from './api.service';
import { Item } from '../models/item';
import { Loan, LoanStatus } from '../models/loan';

describe('ApiService', () => {
  let service: ApiService;
  let httpTestingController: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ApiService,
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(ApiService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('Item operations', () => {
    it('should get all items', () => {
      const mockItems: Item[] = [
        {
          id: 1,
          name: 'Item 1',
          description: 'Desc 1',
          category: 'Tools',
          isAvailable: true,
          ownerId: 1
        },
        {
          id: 2,
          name: 'Item 2',
          description: 'Desc 2',
          category: 'Books',
          isAvailable: false,
          ownerId: 2
        }
      ];

      service.getItems().subscribe(items => {
        expect(items).toEqual(mockItems);
        expect(items.length).toBe(2);
      });

      const req = httpTestingController.expectOne('http://localhost:7001/api/items');
      expect(req.request.method).toEqual('GET');
      req.flush(mockItems);
    });

    it('should get item by id', () => {
      const mockItem: Item = {
        id: 1,
        name: 'Item 1',
        description: 'Desc 1',
        category: 'Tools',
        isAvailable: true,
        ownerId: 1
      };

      service.getItem(1).subscribe(item => {
        expect(item).toEqual(mockItem);
      });

      const req = httpTestingController.expectOne('http://localhost:7001/api/items/1');
      expect(req.request.method).toEqual('GET');
      req.flush(mockItem);
    });

    it('should create item', () => {
      const newItem: Omit<Item, "id" | "ownerId"> = {
        name: 'New Item',
        description: 'New Desc',
        category: 'Electronics',
        isAvailable: true
      };

      const mockResponse: Item = {
        id: 3,
        name: 'New Item',
        description: 'New Desc',
        category: 'Electronics',
        isAvailable: true,
        ownerId: 1
      };

      service.createItem(newItem).subscribe(item => {
        expect(item).toEqual(mockResponse);
      });

      const req = httpTestingController.expectOne('http://localhost:7001/api/items');
      expect(req.request.method).toEqual('POST');
      expect(req.request.body).toEqual(newItem);
      req.flush(mockResponse);
    });

    it('should update item', () => {
      const item: Item = {
        id: 1,
        name: 'Updated Item',
        description: 'Updated Desc',
        category: 'Tools',
        isAvailable: true,
        ownerId: 1
      };

      service.updateItem(1, item).subscribe(response => {
        expect(response).toBeNull();
      });

      const req = httpTestingController.expectOne('http://localhost:7001/api/items/1');
      expect(req.request.method).toEqual('PUT');
      expect(req.request.body).toEqual(item);
      req.flush(null);
    });

    it('should delete item', () => {
      service.deleteItem(1).subscribe(response => {
        expect(response).toBeNull();
      });

      const req = httpTestingController.expectOne('http://localhost:7001/api/items/1');
      expect(req.request.method).toEqual('DELETE');
      req.flush(null);
    });

    it('should get user items', () => {
      const mockItems: Item[] = [
        {
          id: 1,
          name: 'User Item 1',
          description: 'Desc 1',
          category: 'Tools',
          isAvailable: true,
          ownerId: 1
        },
        {
          id: 2,
          name: 'User Item 2',
          description: 'Desc 2',
          category: 'Books',
          isAvailable: false,
          ownerId: 1
        }
      ];

      service.getUserItems().subscribe(items => {
        expect(items).toEqual(mockItems);
        expect(items.length).toBe(2);
      });

      const req = httpTestingController.expectOne('http://localhost:7001/api/items/user');
      expect(req.request.method).toEqual('GET');
      req.flush(mockItems);
    });
  });

  describe('Loan operations', () => {
    it('should get all loans', () => {
      const today = new Date();
      const nextWeek = new Date(today);
      nextWeek.setDate(today.getDate() + 7);

      const mockLoans: Loan[] = [
        {
          id: 1,
          itemId: 1,
          borrowerId: 2,
          startDate: today,
          endDate: nextWeek,
          status: LoanStatus.Requested
        },
        {
          id: 2,
          itemId: 2,
          borrowerId: 1,
          startDate: today,
          endDate: nextWeek,
          status: LoanStatus.Active
        }
      ];

      service.getLoans().subscribe(loans => {
        expect(loans).toEqual(mockLoans);
        expect(loans.length).toBe(2);
      });

      const req = httpTestingController.expectOne('http://localhost:7001/api/loans');
      expect(req.request.method).toEqual('GET');
      req.flush(mockLoans);
    });

    it('should get user loans', () => {
      const today = new Date();
      const nextWeek = new Date(today);
      nextWeek.setDate(today.getDate() + 7);

      const mockLoans: Loan[] = [
        {
          id: 1,
          itemId: 2,
          borrowerId: 1,
          startDate: today,
          endDate: nextWeek,
          status: LoanStatus.Active
        }
      ];

      service.getUserLoans().subscribe(loans => {
        expect(loans).toEqual(mockLoans);
        expect(loans.length).toBe(1);
      });

      const req = httpTestingController.expectOne('http://localhost:7001/api/loans/user');
      expect(req.request.method).toEqual('GET');
      req.flush(mockLoans);
    });

    it('should get loans for owner', () => {
      const today = new Date();
      const nextWeek = new Date(today);
      nextWeek.setDate(today.getDate() + 7);

      const mockLoans: Loan[] = [
        {
          id: 1,
          itemId: 1,
          borrowerId: 2,
          startDate: today,
          endDate: nextWeek,
          status: LoanStatus.Requested
        }
      ];

      service.getOwnerLoans().subscribe(loans => {
        expect(loans).toEqual(mockLoans);
        expect(loans.length).toBe(1);
      });

      const req = httpTestingController.expectOne('http://localhost:7001/api/loans/owner');
      expect(req.request.method).toEqual('GET');
      req.flush(mockLoans);
    });

    it('should create loan', () => {
      const today = new Date();
      const nextWeek = new Date(today);
      nextWeek.setDate(today.getDate() + 7);

      const newLoan: Loan = {
        itemId: 1,
        borrowerId: 2,
        startDate: today,
        endDate: nextWeek,
        status: LoanStatus.Requested,
        notes: 'Test loan'
      };

      const mockResponse: Loan = {
        id: 1,
        itemId: 1,
        borrowerId: 2,
        startDate: today,
        endDate: nextWeek,
        status: LoanStatus.Requested,
        notes: 'Test loan'
      };

      service.createLoan(newLoan).subscribe(loan => {
        expect(loan).toEqual(mockResponse);
      });

      const req = httpTestingController.expectOne('http://localhost:7001/api/loans');
      expect(req.request.method).toEqual('POST');
      expect(req.request.body).toEqual(newLoan);
      req.flush(mockResponse);
    });

    it('should update loan status', () => {
      service.updateLoanStatus(1, LoanStatus.Approved).subscribe(response => {
        expect(response).toBeNull();
      });

      const req = httpTestingController.expectOne('http://localhost:7001/api/loans/1/status');
      expect(req.request.method).toEqual('PUT');
      expect(req.request.body).toEqual('1');
      req.flush(null);
    });

    it('should initiate return', () => {
      service.initiateReturn(1).subscribe(response => {
        expect(response).toBeNull();
      });

      const req = httpTestingController.expectOne('http://localhost:7001/api/loans/1/initiate-return');
      expect(req.request.method).toEqual('PUT');
      expect(req.request.body).toEqual({});
      req.flush(null);
    });

    it('should confirm return', () => {
      service.confirmReturn(1).subscribe(response => {
        expect(response).toBeNull();
      });

      const req = httpTestingController.expectOne('http://localhost:7001/api/loans/1/confirm-return');
      expect(req.request.method).toEqual('PUT');
      expect(req.request.body).toEqual({});
      req.flush(null);
    });

    it('should approve loan request', () => {
      service.approveRequest(1).subscribe(response => {
        expect(response).toBeNull();
      });

      const req = httpTestingController.expectOne('http://localhost:7001/api/loans/1/approve');
      expect(req.request.method).toEqual('PUT');
      expect(req.request.body).toEqual({});
      req.flush(null);
    });

    it('should reject loan request', () => {
      service.rejectRequest(1).subscribe(response => {
        expect(response).toBeNull();
      });

      const req = httpTestingController.expectOne('http://localhost:7001/api/loans/1/reject');
      expect(req.request.method).toEqual('PUT');
      expect(req.request.body).toEqual({});
      req.flush(null);
    });

    it('should activate loan', () => {
      service.activateLoan(1).subscribe(response => {
        expect(response).toBeNull();
      });

      const req = httpTestingController.expectOne('http://localhost:7001/api/loans/1/activate');
      expect(req.request.method).toEqual('PUT');
      expect(req.request.body).toEqual({});
      req.flush(null);
    });
  });
});
