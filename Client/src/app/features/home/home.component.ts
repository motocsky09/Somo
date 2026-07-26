import { Component } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';

interface FaqItem {
  question: string;
  answer: string;
  open: boolean;
}

interface PetStore {
  name: string;
  url: string;
  tagline: string;
}

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {
  stores: PetStore[] = [
    { name: 'zooplus', url: 'https://www.zooplus.ro', tagline: 'Cel mai mare magazin online de animale' },
    { name: 'maxipet', url: 'https://www.maxipet.ro', tagline: 'Tot ce ai nevoie pentru prietenul tău' },
    { name: 'animax', url: 'https://www.animax.ro', tagline: 'Hrană și accesorii premium' }
  ];

  faqs: FaqItem[] = [
    {
      question: 'Cum găsesc un cabinet veterinar aproape de mine?',
      answer: 'Mergi în secțiunea "Cabinete", permite accesul la locație și vei vedea pe hartă toate cabinetele din raza aleasă de tine. Poți ajusta raza de căutare și vedea detalii despre fiecare cabinet.',
      open: true
    },
    {
      question: 'Cum îmi înrolez animalul de companie?',
      answer: 'Din secțiunea "Animalele mele" apeși pe "Adaugă animal", completezi numele, specia, rasa și data nașterii. Vei putea apoi să faci programări și să urmărești istoricul medical al fiecărui animal.',
      open: false
    },
    {
      question: 'Cum fac o programare la cabinet?',
      answer: 'Alegi un cabinet de pe hartă sau mergi la "Programare nouă", selectezi animalul, cabinetul, data și ora dorită. Programarea va apărea în "Programările mele" unde poți urmări statusul ei.',
      open: false
    },
    {
      question: 'Pot vedea istoricul medical al animalului meu?',
      answer: 'Da. Fiecare animal înrolat pe contul tău are un istoric al programărilor și al vizitelor efectuate, accesibil oricând din contul tău.',
      open: false
    },
    {
      question: 'Utilizarea aplicației Somo este gratuită?',
      answer: 'Da, crearea contului, înrolarea animalelor și căutarea cabinetelor sunt complet gratuite. Plătești doar serviciile veterinare direct la cabinet.',
      open: false
    }
  ];

  constructor(public authService: AuthService) {}

  toggleFaq(item: FaqItem): void {
    item.open = !item.open;
  }
}
