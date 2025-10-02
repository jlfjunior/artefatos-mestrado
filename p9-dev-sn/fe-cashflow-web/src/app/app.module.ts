import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from "@angular/common/http";
import { NgModule, LOCALE_ID } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { CommonModule,registerLocaleData  } from '@angular/common';
import { BrowserModule } from "@angular/platform-browser";
import { RouterModule } from "@angular/router";

import { routes } from './app.routes';
import { AppComponent } from "./app.component";
import { LoginComponent } from "./pages/login/login.component";
import { LayoutComponent } from "./layouts/layout/layout.component";
import { HeaderComponent } from "./layouts/header/header.component";
import { SidebarComponent } from "./layouts/sidebar/sidebar.component";
import { DashboardComponent } from "./pages/dashboard/dashboard.component";
import { TransactionsComponent } from "./pages/transactions/transactions.component";
import { ReportsComponent } from "./pages/reports/reports.component";
import { TransactionService } from "./services/transaction.service";
import { ReportService } from "./services/report.service";
import { BaseChartDirective, provideCharts, withDefaultRegisterables } from "ng2-charts";
import { ManagementService } from "./services/management.service";
import localePt from '@angular/common/locales/pt';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ToastrModule } from 'ngx-toastr';
import { AuthInterceptor } from "./shared/auth.interceptor";

registerLocaleData(localePt);

@NgModule({ declarations: [
        AppComponent,
        LayoutComponent,
        HeaderComponent,
        SidebarComponent,
        LoginComponent,
        DashboardComponent,
        TransactionsComponent,
        ReportsComponent
    ],
    bootstrap: [AppComponent], imports: [RouterModule.forRoot(routes),
        BrowserModule,
        FormsModule,
        CommonModule,
        ReactiveFormsModule,
        BaseChartDirective,
        BrowserAnimationsModule,
        ToastrModule.forRoot({
            positionClass: 'toast-top-right',
            timeOut: 3000,
            closeButton: true,
            progressBar: true,
            progressAnimation: 'increasing',
            preventDuplicates: true,
        })], providers: [
        TransactionService,
        ManagementService,
        ReportService,
        provideCharts(withDefaultRegisterables()),
        {
            provide: LOCALE_ID, useValue: 'pt-BR'
        },
        {
            provide: HTTP_INTERCEPTORS,
            useClass: AuthInterceptor,
            multi: true
        },
        provideHttpClient(withInterceptorsFromDi())
    ] })
export class AppModule { }