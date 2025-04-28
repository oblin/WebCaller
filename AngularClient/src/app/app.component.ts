import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { environment } from '../environments/environment';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
})
export class AppComponent implements OnInit {
  connections: { connectionId: string, clientName: string }[] = [];
  selectedConnection = '';
  action: string = '';
  serverUrl: string = 'http://localhost:5261'; // WebCaller URL
  errorMessage: string = '';

  constructor(private http: HttpClient) {}

  ngOnInit() {
    if (environment.serverUrl) {
      this.serverUrl = environment.serverUrl; // WebCaller URL
    }
  
    this.loadConnections();

    // Refresh every 30 seconds
    setInterval(() => {
      this.loadConnections();
    }, 30000);
  }

  loadConnections() {
    this.errorMessage = ''; // Reset error message
    this.http.get<{ connectionId: string, clientName: string }[]>(`${this.serverUrl}/invoke/connections`)
      .subscribe({
        next: (data) => {
          this.connections = data;
        },
        error: (error) => {
          console.error('Failed to fetch connections', error);
          this.connections = [];          // <--- Clear dropdown when failed
          this.selectedConnection = ''; // <--- Clear selected value too
          this.errorMessage = 'Failed to fetch connections. Server might be down.';
        }
      });
  }

  sendCommand() {
    if (!this.selectedConnection) {
      alert('Please select a connection and enter an action.');
      return;
    }

    this.errorMessage = ''; 
    const params = {
      connectionId: this.selectedConnection,
      action: this.action
    };

    this.http.post(`${this.serverUrl}/invoke`, params)
      .subscribe({
        next: () => {
          alert('Command sent successfully.');
        },
        error: (err) => {
          console.error(err);
          this.errorMessage = 'Failed to invoke action.';
        }
      });
  }
}
