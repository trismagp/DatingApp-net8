import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';

import { Member } from '../_models/member';
import { environment } from '../../environments/environment';
import { of, tap } from 'rxjs';
import { Photo } from '../_models/photo';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  private http = inject(HttpClient);
  baseUrl = environment.apiUrl;
  members = signal<Member[]>([]);


  getMembers() {
    return this.http.get<Member[]>(this.baseUrl + 'users').subscribe({
      next: members => this.members.set(members)
    });


  }
  getMember(username: string) {
    const member = this.members().find(x=>x.username===username);
    // ==> of(member) return a member as observable
    if (member !== undefined) return of(member); 
    
    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }

  updateMember(member: Member) {
    return this.http.put(this.baseUrl + 'users', member).pipe(
      tap(() => {
        this.members.update(members => 
          members.map(m => 
            m.username === member.username 
            ? member : m))
      })
    )
  }

  setMainPhoto(photo: Photo) {
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photo.id, {}).pipe(
      tap(() => {
        this.members.update(members => members.map(m => {
          if (m.photos.includes(photo)) {
            m.photoUrl = photo.url
          }
          return m;
        }))
      })
    )
  }


}