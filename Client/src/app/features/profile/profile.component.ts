import { Component, ElementRef, HostListener, OnInit, ViewChild } from '@angular/core';
import { AuthService, UserProfile } from '../../core/services/auth.service';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  profile: UserProfile | null = null;

  isLoading = true;
  isSaving = false;
  errorMessage = '';
  successMessage = '';

  firstName = '';
  lastName = '';
  email = '';
  phone = '';
  photoUrl: string | null = null;

  currentPassword = '';
  newPassword = '';
  confirmPassword = '';
  isChangingPassword = false;
  passwordError = '';
  passwordSuccess = '';

  isCropping = false;
  cropSrc = '';
  cropZoom = 1;
  minZoom = 1;
  maxZoom = 3;
  offsetX = 0;
  offsetY = 0;
  dispW = 0;
  dispH = 0;
  readonly viewport = 260;

  private readonly output = 400;
  private cropImg: HTMLImageElement | null = null;
  private baseScale = 1;
  private dragging = false;
  private lastPx = 0;
  private lastPy = 0;

  @ViewChild('photoInput') photoInput!: ElementRef<HTMLInputElement>;

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    this.authService.getProfile().subscribe({
      next: profile => {
        this.applyProfile(profile);
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Datele contului nu au putut fi încărcate.';
        this.isLoading = false;
      }
    });
  }

  get isVet(): boolean {
    return this.authService.isVet;
  }

  get initials(): string {
    const source = [this.firstName, this.lastName].filter(n => !!n.trim());
    if (source.length === 0) return (this.profile?.username || '?').charAt(0).toUpperCase();
    return source.map(n => n.trim().charAt(0).toUpperCase()).join('');
  }

  get displayName(): string {
    const fullName = [this.firstName, this.lastName].filter(n => !!n.trim()).join(' ');
    return fullName || this.profile?.username || '';
  }

  changePassword(): void {
    this.passwordError = '';
    this.passwordSuccess = '';

    if (!this.currentPassword || !this.newPassword) {
      this.passwordError = 'Completează parola curentă și pe cea nouă.';
      return;
    }
    if (this.newPassword.length < 6) {
      this.passwordError = 'Parola nouă trebuie să aibă cel puțin 6 caractere.';
      return;
    }
    if (this.newPassword !== this.confirmPassword) {
      this.passwordError = 'Parolele nu coincid.';
      return;
    }

    this.isChangingPassword = true;
    this.authService.changePassword(this.currentPassword, this.newPassword).subscribe({
      next: () => {
        this.passwordSuccess = 'Parola a fost schimbată.';
        this.isChangingPassword = false;
        this.currentPassword = '';
        this.newPassword = '';
        this.confirmPassword = '';
      },
      error: err => {
        this.passwordError = err?.error?.message || 'Parola nu a putut fi schimbată.';
        this.isChangingPassword = false;
      }
    });
  }

  save(): void {
    if (!this.email.trim()) {
      this.errorMessage = 'Adresa de email este obligatorie.';
      return;
    }
    if (this.phone.trim() && !/^[0-9+\s().-]{6,20}$/.test(this.phone.trim())) {
      this.errorMessage = 'Numărul de telefon nu are un format valid.';
      return;
    }
    this.persist();
  }

  onAvatarClick(): void {
    if (this.photoUrl) {
      this.initCrop(this.photoUrl);
    } else {
      this.photoInput.nativeElement.click();
    }
  }

  triggerChangePhoto(): void {
    this.photoInput.nativeElement.click();
  }

  onPhotoSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files && input.files[0];
    if (!file) return;

    if (!file.type.startsWith('image/')) {
      this.errorMessage = 'Fișierul selectat nu este o imagine.';
      return;
    }

    const reader = new FileReader();
    reader.onload = () => this.initCrop(reader.result as string);
    reader.readAsDataURL(file);
    input.value = '';
  }

  setZoom(newZoom: number): void {
    const z = Math.min(this.maxZoom, Math.max(this.minZoom, newZoom));
    const oldEff = this.baseScale * this.cropZoom;
    const newEff = this.baseScale * z;
    const centerX = (this.viewport / 2 - this.offsetX) / oldEff;
    const centerY = (this.viewport / 2 - this.offsetY) / oldEff;
    this.cropZoom = z;
    this.applyDims();
    this.offsetX = this.viewport / 2 - centerX * newEff;
    this.offsetY = this.viewport / 2 - centerY * newEff;
    this.clampOffsets();
  }

  onDragStart(event: PointerEvent): void {
    event.preventDefault();
    this.dragging = true;
    this.lastPx = event.clientX;
    this.lastPy = event.clientY;
  }

  @HostListener('document:pointermove', ['$event'])
  onDragMove(event: PointerEvent): void {
    if (!this.dragging) return;
    this.offsetX += event.clientX - this.lastPx;
    this.offsetY += event.clientY - this.lastPy;
    this.lastPx = event.clientX;
    this.lastPy = event.clientY;
    this.clampOffsets();
  }

  @HostListener('document:pointerup')
  onDragEnd(): void {
    this.dragging = false;
  }

  onWheel(event: WheelEvent): void {
    event.preventDefault();
    this.setZoom(this.cropZoom + (event.deltaY < 0 ? 0.15 : -0.15));
  }

  cancelCrop(): void {
    this.isCropping = false;
    this.cropImg = null;
    this.cropSrc = '';
  }

  confirmCrop(): void {
    if (!this.cropImg) return;
    const eff = this.baseScale * this.cropZoom;
    const sourceSize = this.viewport / eff;

    const canvas = document.createElement('canvas');
    canvas.width = this.output;
    canvas.height = this.output;
    canvas.getContext('2d')?.drawImage(
      this.cropImg,
      -this.offsetX / eff, -this.offsetY / eff, sourceSize, sourceSize,
      0, 0, this.output, this.output
    );

    this.photoUrl = canvas.toDataURL('image/jpeg', 0.85);
    this.persist(() => this.cancelCrop());
  }

  removePhoto(): void {
    this.photoUrl = null;
    this.persist(() => this.cancelCrop());
  }

  private initCrop(src: string): void {
    const img = new Image();
    img.onload = () => {
      this.cropImg = img;
      this.cropSrc = src;
      this.cropZoom = 1;
      this.baseScale = Math.max(this.viewport / img.width, this.viewport / img.height);
      this.applyDims();
      this.offsetX = (this.viewport - this.dispW) / 2;
      this.offsetY = (this.viewport - this.dispH) / 2;
      this.errorMessage = '';
      this.isCropping = true;
    };
    img.src = src;
  }

  private applyDims(): void {
    const eff = this.baseScale * this.cropZoom;
    this.dispW = (this.cropImg?.width || 0) * eff;
    this.dispH = (this.cropImg?.height || 0) * eff;
  }

  private clampOffsets(): void {
    this.offsetX = Math.min(0, Math.max(this.viewport - this.dispW, this.offsetX));
    this.offsetY = Math.min(0, Math.max(this.viewport - this.dispH, this.offsetY));
  }

  private persist(onDone?: () => void): void {
    this.isSaving = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.authService.updateProfile({
      firstName: this.firstName.trim(),
      lastName: this.lastName.trim(),
      email: this.email.trim(),
      phone: this.phone.trim(),
      profilePhotoUrl: this.photoUrl
    }).subscribe({
      next: profile => {
        this.applyProfile(profile);
        this.isSaving = false;
        this.successMessage = 'Datele de contact au fost salvate.';
        setTimeout(() => this.successMessage = '', 3000);
        onDone?.();
      },
      error: err => {
        this.errorMessage = err?.error?.message || 'Datele nu au putut fi salvate. Încearcă din nou.';
        this.isSaving = false;
      }
    });
  }

  private applyProfile(profile: UserProfile): void {
    this.profile = profile;
    this.firstName = profile.firstName || '';
    this.lastName = profile.lastName || '';
    this.email = profile.email || '';
    this.phone = profile.phone || '';
    this.photoUrl = profile.profilePhotoUrl || null;
  }
}
