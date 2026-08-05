# Somo (.NET Core & Angular)
Technology and libraries used:
```
1. Server: .NET Core, Identity with JWT Token, C#, MongoDB, Google Places API
```
```
2. Client: Angular, TypeScript, HTML & CSS, Google Maps
```
Design patterns and principles used:
```
Clean Architecture
```
```
REST and Dependency Injection
```
```
OOP and S.O.L.I.D Principles
```
### Somo is a veterinary management platform developed on a modern [ASP.NET](http://asp.net/) Core architecture, designed to streamline the interaction between pet owners and veterinary clinics.

The platform serves three kinds of accounts: **pet owners** enrol their animals, find a clinic and book visits; **clinic owners** register their practice, manage their vets and handle incoming appointments; a **Somo administrator** reviews every clinic request before the practice becomes visible on the map.

Every screen is responsive, so the same account works the same way on a desktop, a tablet or a phone. Each section below shows the desktop view first and the phone view after it.

# Pet owner

## Home page

The public landing page presents the platform, what it offers, a feeding guide and the most frequent questions.

![Home page](https://github.com/motocsky09/Somo/raw/main/_screens/home-1.png)

![Home page](https://github.com/motocsky09/Somo/raw/main/_screens/home-2.png)

On a phone the two columns of the hero stack, the cards become a single column and the navigation collapses behind a menu button.

![Home page on a phone](https://github.com/motocsky09/Somo/raw/main/_screens/phone/home-1.png)

![Home page on a phone](https://github.com/motocsky09/Somo/raw/main/_screens/phone/home-2.png)

![Home page on a phone](https://github.com/motocsky09/Somo/raw/main/_screens/phone/home-3.png)

![Home page on a phone](https://github.com/motocsky09/Somo/raw/main/_screens/phone/home-4.png)

![Navigation menu on a phone](https://github.com/motocsky09/Somo/raw/main/_screens/phone/menu.png)

## Login page

![Login page](https://github.com/motocsky09/Somo/raw/main/_screens/login.png)

![Login page on a phone](https://github.com/motocsky09/Somo/raw/main/_screens/phone/login.png)

## Create an account

![Register page](https://github.com/motocsky09/Somo/raw/main/_screens/register-pet-owner.png)

![Register page on a phone](https://github.com/motocsky09/Somo/raw/main/_screens/phone/register-pet-owner.png)

## Find a vet clinic

Clinics are searched by city or around the current location. The results combine the practices registered on Somo with the ones returned by Google Places, so the map stays useful even where the platform has not arrived yet.

![Vet page](https://github.com/motocsky09/Somo/raw/main/_screens/vet-clinics.png)

On a phone the map and the result list sit one under the other instead of side by side.

![Vet page on a phone](https://github.com/motocsky09/Somo/raw/main/_screens/phone/find-clinic.png)

![Vet search on a phone](https://github.com/motocsky09/Somo/raw/main/_screens/phone/find-clinic-search.png)

## My pets page

Each animal has its own profile with species, breed, age, weight, a photo cropped in the browser and the history of its visits.

![Pets page](https://github.com/motocsky09/Somo/raw/main/_screens/my-pet.png)

![Pet page](https://github.com/motocsky09/Somo/raw/main/_screens/pet-profile.png)

![Pets page on a phone](https://github.com/motocsky09/Somo/raw/main/_screens/phone/my-pets.png)

![Pet page on a phone](https://github.com/motocsky09/Somo/raw/main/_screens/phone/pet-profile.png)

## Appointments page

The owner picks the animal, the clinic, the vet and a free time slot, then follows the status of the visit until it is completed.

![Appointments page](https://github.com/motocsky09/Somo/raw/main/_screens/my-appointments.png)

![Appointments page on a phone](https://github.com/motocsky09/Somo/raw/main/_screens/phone/my-appointments.png)

## My account page

The contact details filled in here travel with every booking, so the clinic knows who to call about a visit.

![Account page](https://github.com/motocsky09/Somo/raw/main/_screens/pet-owner-profile.png)

# Clinic owner

## Clinic sign up

A clinic account is created together with the practice itself: address, contact data, schedule, vets and price list are collected in the same step and sent for review.

![Clinic sign up](https://github.com/motocsky09/Somo/raw/main/_screens/register-vet-clinic-1.png)

![Clinic sign up](https://github.com/motocsky09/Somo/raw/main/_screens/register-vet-clinic-2.png)

![Clinic sign up on a phone](https://github.com/motocsky09/Somo/raw/main/_screens/phone/register-vet-clinic.png)

## Clinic dashboard

Once the request is approved, the dashboard shows the incoming appointments with the animal, its owner and the reason for the visit, the pets scheduled at the practice and the vets working there.

![Clinic dashboard](https://github.com/motocsky09/Somo/raw/main/_screens/vet-clinic-dashboard.png)

On a phone the four counters and the two panels stack into one column.

![Clinic dashboard on a phone](https://github.com/motocsky09/Somo/raw/main/_screens/phone/vet-clinic-dashboard-1.png)

![Clinic dashboard on a phone](https://github.com/motocsky09/Somo/raw/main/_screens/phone/vet-clinic-dashboard-2.png)

## Appointment details

The clinic can confirm, reschedule, reassign or complete a visit, and has the owner's phone and email one click away.

![Appointment details](https://github.com/motocsky09/Somo/raw/main/_screens/vet-clinic-appointment.png)

![Appointment details on a phone](https://github.com/motocsky09/Somo/raw/main/_screens/phone/vet-clinic-appointment-1.png)

![Appointment details on a phone](https://github.com/motocsky09/Somo/raw/main/_screens/phone/vet-clinic-appointment-2.png)

# Somo administrator

## Clinic review

Every clinic request is checked before it goes live, and a rejected one goes back to the clinic owner with the reason attached.

![Admin dashboard](https://github.com/motocsky09/Somo/raw/main/_screens/admin-dashboard.png)

The requests, the approved clinics and the pet owners with their animals are split across three tabs, which stay usable on a phone.

![Admin clinics on a phone](https://github.com/motocsky09/Somo/raw/main/_screens/phone/admin-clinics.png)

![Admin owners on a phone](https://github.com/motocsky09/Somo/raw/main/_screens/phone/admin-owners.png)
