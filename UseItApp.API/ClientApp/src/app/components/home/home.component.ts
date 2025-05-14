import {Component} from '@angular/core';
import {RouterLink} from '@angular/router';
import {MatIconModule} from "@angular/material/icon";
import {MatCardContent, MatCardModule} from "@angular/material/card";
import {MatButtonModule} from "@angular/material/button";

@Component({
    selector: 'app-home',
    templateUrl: './home.component.html',
    styleUrl: './home.component.css',
    standalone: true,
    imports: [
        RouterLink,
        MatCardContent,
        MatCardModule,
        MatIconModule,
        MatButtonModule
    ],
})
export class HomeComponent {

}
