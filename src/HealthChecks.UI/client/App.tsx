import React from "react";
import { Route, Link, Redirect } from "react-router-dom";
import { LivenessPage } from "./components/LivenessPage";
import { WebhooksPage } from "./components/WebhooksPage";
import { scaleRotate as Menu } from "react-burger-menu";

interface AppProps {    
    apiEndpoint: string;
    webhookEndpoint: string;
}

interface AppState {
    menuOpen: boolean;
}

const WhiteGearIcon = require("../assets/svg/white-gear.svg");
const WhiteHeartIcon = require("../assets/svg/white-heart.svg");

export class App extends React.Component<AppProps, AppState> {
    constructor(props: AppProps) {
        super(props);
        this.state = {
            menuOpen: false
        };

        this.toggleMenu = this.toggleMenu.bind(this);
    }

    toggleMenu() {
        this.setState({
            menuOpen: !this.state.menuOpen
        });
    }
    render() {
        return <React.Fragment>
            <div id="outer-container" style={{ height: '100%' }}>
                <Menu onStateChange={(state) => this.setState({ menuOpen: state.isOpen })}
                    isOpen={this.state.menuOpen}
                    pageWrapId={'wrapper'}
                    outerContainerId={"outer-container"} >
                    <Link to={'/healthchecks'} className="menu-item" onClick={this.toggleMenu}>
                        <img className="menu-icon" src={WhiteHeartIcon} />
                        <div>Health Checks</div>
                    </Link>
                    <Link to={'/webhooks'} className="menu-item" onClick={this.toggleMenu}>
                        <img className="menu-icon" src={WhiteGearIcon} />
                        <div>Webhooks</div>
                    </Link>
                </Menu>
                <Route exact path="/" render={() => <Redirect to="/healthchecks" />} />
                <Route path={'/healthchecks'} render={() => <LivenessPage endpoint={this.props.apiEndpoint} />} />                
                <Route path={'/webhooks'} render={() => <WebhooksPage endpoint={this.props.webhookEndpoint} />} />
            </div>            
        </React.Fragment>
    }
}