import LandingNavbar from "./landing-navbar";
import './landing-main.css'
import LandingFooter from "./landing-footer";
import Button from '@mui/material/Button';

function LandingMain() {
    return (<>
      <header className="landing-navbar">
        <LandingNavbar/>
      </header>
      <main className="landing-main flex-row-center">
          <section>
            <Button size="large" color="primary" variant="text">Get Started</Button>
          </section>
      </main>
      <footer className="landing-footer">
        <LandingFooter/>
      </footer>
    </>);
}

export default LandingMain;