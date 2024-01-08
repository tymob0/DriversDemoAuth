import { HttpEvent } from "@angular/common/http";
import { HttpHandler } from "@angular/common/http";
import { HttpInterceptor } from "@angular/common/http";
import { HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";

@Injectable()
export class AuthenticationInterceptor implements HttpInterceptor{
    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        const token = localStorage.getItem('jwtToken');

        if(token){
            req = req.clone({
                setHeaders: {Authorization: `Bearer ${token}`}
            });
        }
        return next.handle(req);
    }
}
